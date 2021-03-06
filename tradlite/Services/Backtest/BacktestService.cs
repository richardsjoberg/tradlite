﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models.Backtesting;
using Tradlite.Models.Requests;
using Tradlite.Services.Candle.CandleService;
using Tradlite.Services.Ig;
using Tradlite.Services.Management;
using Tradlite.Services.Signals;
using Dapper.Contrib.Extensions;
using Tradlite.Models;
using Trady.Core.Infrastructure;
using Tradlite.Services.SqlConnectionFactory;

namespace Tradlite.Services.Backtest
{
    public interface IBacktestService
    {
        Task<List<Models.Backtesting.Transaction>> Run(IReadOnlyList<IOhlcv> candles, BacktestConfig backtestConfig, decimal minSize, string ticker, decimal exchangeRate, decimal risk);
    }
    public class BacktestService : IBacktestService
    {
        private readonly Func<string, IStopLossManagement> _stopLossManagementAccessor;
        private readonly Func<string, IEntryManagement> _entrytManagementAccessor;
        private readonly Func<string, ILimitManagement> _limitManagementAccessor;
        private readonly Func<string, ISignalService> _signalServiceAccessor;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly Func<string, IEntryFilterManagement> _entryFilterManagementAccessor;

        public BacktestService(ICandleService candleService,
            Func<string, IStopLossManagement> stopLossManagementAccessor,
            Func<string, IEntryManagement> entrytManagementAccessor,
            Func<string, ILimitManagement> limitManagementAccessor,
            IIgService igService,
            Func<string, ISignalService> signalServiceAccessor,
            ISqlConnectionFactory sqlConnectionFactory,
            Func<string, IEntryFilterManagement> entryFilterManagementAccessor)
        {
            _stopLossManagementAccessor = stopLossManagementAccessor;
            _entrytManagementAccessor = entrytManagementAccessor;
            _limitManagementAccessor = limitManagementAccessor;
            _signalServiceAccessor = signalServiceAccessor;
            _sqlConnectionFactory = sqlConnectionFactory;
            _entryFilterManagementAccessor = entryFilterManagementAccessor;
        }

        public async Task<List<Transaction>> Run(IReadOnlyList<IOhlcv> candles, BacktestConfig backtestConfig, decimal minSize, string ticker, decimal exchangeRate, decimal risk)
        {
            using (var _dbConnection = _sqlConnectionFactory.CreateConnection())
            {
                var signalConfig = await _dbConnection.GetAsync<SignalConfig>(backtestConfig.EntrySignalConfigId);
                var signals = _signalServiceAccessor(backtestConfig.EntrySignalService).GetSignals(candles, signalConfig.Parameters);

                int[] exitSignals = new int[0];
                if (!string.IsNullOrEmpty(backtestConfig.ExitSignalService))
                {
                    var exitSignalConfig = await _dbConnection.GetAsync<SignalConfig>(backtestConfig.ExitSignalConfigId);
                    exitSignals = _signalServiceAccessor(backtestConfig.ExitSignalService).GetSignals(candles, exitSignalConfig.Parameters);
                }

                var positions = new List<Position>();
                var transactions = new List<Transaction>();
                Position currentPosition = null;
                Order currentOrder = null;
                var currentIndex = 0;
                int? lastExitIndex = null;

                while (currentIndex <= candles.Count - 1)
                {
                    var candle = candles[currentIndex];
                    if (currentOrder != null)
                    {
                        if (currentOrder.Direction == OrderDirection.Long &&
                            currentOrder.Type == OrderType.Limit &&
                            currentOrder.EntryLevel > candle.Low)
                        {
                            currentPosition = new Position
                            {
                                Created = candle.DateTime.LocalDateTime,
                                Direction = OrderDirection.Long,
                                EntryLevel = currentOrder.EntryLevel,
                                Size = currentOrder.Size,
                                Stop = currentOrder.Stop
                            };
                            currentOrder = null;
                            currentIndex++;
                            continue;
                        }

                        if (currentOrder.Direction == OrderDirection.Short &&
                            currentOrder.Type == OrderType.Limit &&
                            currentOrder.EntryLevel < candle.High)
                        {
                            currentPosition = new Position
                            {
                                Created = candle.DateTime.LocalDateTime,
                                Direction = OrderDirection.Short,
                                EntryLevel = currentOrder.EntryLevel,
                                Size = currentOrder.Size,
                                Stop = currentOrder.Stop
                            };
                            currentOrder = null;
                            currentIndex++;
                            continue;
                        }
                    }
                    if (currentPosition != null)
                    {
                        if (currentPosition.Direction == OrderDirection.Long)
                        {
                            if (currentPosition.Limit.HasValue && candle.High >= currentPosition.Limit)
                            {
                                transactions.Add(CreateLongTransaction(currentPosition, candle.DateTime.LocalDateTime, currentPosition.Limit.Value, ticker, exchangeRate));
                                currentPosition = null;
                                lastExitIndex = currentIndex;
                            }
                            else if (exitSignals.Contains(currentIndex))
                            {
                                transactions.Add(CreateLongTransaction(currentPosition, candle.DateTime.LocalDateTime, candle.Close, ticker, exchangeRate));
                                currentPosition = null;
                                lastExitIndex = currentIndex;
                            }
                            else if (candle.Low <= currentPosition.Stop)
                            {
                                transactions.Add(CreateLongTransaction(currentPosition, candle.DateTime.LocalDateTime, currentPosition.Stop.Value, ticker, exchangeRate));
                                currentPosition = null;
                                lastExitIndex = currentIndex;
                            } 
                            else if (!string.IsNullOrEmpty(backtestConfig.TrailingStopLossManagement))
                            {
                                var stopLoss = _stopLossManagementAccessor(backtestConfig.TrailingStopLossManagement).StopLoss(candles, currentIndex, ticker, backtestConfig.Parameters);
                                if(stopLoss.HasValue && stopLoss > currentPosition.Stop)
                                {
                                    currentPosition.Stop = stopLoss;
                                }
                            }
                        }
                        else if (currentPosition.Direction == OrderDirection.Short)
                        {
                            if (currentPosition.Limit.HasValue && candle.Low <= currentPosition.Limit)
                            {
                                transactions.Add(CreateShortTransaction(currentPosition, candle.DateTime.LocalDateTime, currentPosition.Limit.Value, ticker, exchangeRate));
                                currentPosition = null;
                                lastExitIndex = currentIndex;
                            }
                            else if (exitSignals.Contains(currentIndex))
                            {
                                transactions.Add(CreateShortTransaction(currentPosition, candle.DateTime.LocalDateTime, candle.Close, ticker, exchangeRate));
                                currentPosition = null;
                                lastExitIndex = currentIndex;
                            }
                            else if (candle.High >= currentPosition.Stop)
                            {
                                transactions.Add(CreateShortTransaction(currentPosition, candle.DateTime.LocalDateTime, currentPosition.Stop.Value, ticker, exchangeRate));
                                currentPosition = null;
                                lastExitIndex = currentIndex;
                            }
                            else if(!string.IsNullOrEmpty(backtestConfig.TrailingStopLossManagement))
                            {
                                var stopLoss = _stopLossManagementAccessor(backtestConfig.TrailingStopLossManagement).StopLoss(candles, currentIndex, ticker, backtestConfig.Parameters);
                                if (stopLoss.HasValue && stopLoss < currentPosition.Stop)
                                {
                                    currentPosition.Stop = stopLoss;
                                }
                            }
                        }
                        currentIndex++;
                        continue;
                    }

                    if (signals.Any(s => s == currentIndex))
                    {
                        if (lastExitIndex.HasValue && signals.Count(s => s > lastExitIndex.Value && s <= currentIndex) == currentIndex - lastExitIndex.Value)
                        {
                            currentIndex++;
                            continue;
                        }
                            
                        if(!string.IsNullOrEmpty(backtestConfig.EntryFilterManagement))
                        {
                            if (!_entryFilterManagementAccessor(backtestConfig.EntryFilterManagement).Entry(candles, currentIndex, ticker, backtestConfig.Parameters))
                            {
                                currentIndex++;
                                continue;
                            }
                        }
                        var stop = _stopLossManagementAccessor(backtestConfig.StopLossManagement).StopLoss(candles, currentIndex, ticker, backtestConfig.Parameters);
                        var entry = _entrytManagementAccessor(backtestConfig.EntryManagement).Entry(candles, currentIndex, ticker, backtestConfig.Parameters);
                        decimal? limit = null;
                        if (!string.IsNullOrEmpty(backtestConfig.LimitManagement))
                        {
                            limit = _limitManagementAccessor(backtestConfig.LimitManagement).Limit(candles, currentIndex, ticker, backtestConfig.Parameters);
                        }

                        if (currentIndex + 1 > candles.Count - 1 || !stop.HasValue || !entry.HasValue)
                        {
                            currentIndex++;
                            continue;
                        }

                        Enum.TryParse(backtestConfig.Direction, out OrderDirection direction);
                        Enum.TryParse(backtestConfig.OrderType, out OrderType orderType);
                        var stopSize = Math.Abs(entry.Value - stop.Value) * exchangeRate * minSize;
                        int size = (int)(risk / stopSize);

                        if (size == 0)
                        {
                            currentIndex++;
                            continue;
                        }

                        if (orderType == OrderType.Market)
                        {
                            currentPosition = new Position
                            {
                                Created = candle.DateTime.LocalDateTime,
                                Direction = direction,
                                EntryLevel = entry.Value,
                                Limit = limit,
                                Size = size * minSize,
                                Stop = stop
                            };
                        }
                        else
                        {
                            currentOrder = new Order
                            {
                                Direction = direction,
                                EntryLevel = entry.Value,
                                Created = candle.DateTime.LocalDateTime,
                                Size = minSize,
                                Stop = stop.Value,
                                Type = orderType,
                                Limit = limit
                            };
                        }
                    }

                    currentIndex++;
                }
                return transactions;
            }
        }

        private Transaction CreateLongTransaction(Position currentPosition, DateTime exitDate, decimal exitLevel, string ticker, decimal exchangeRate)
        {
            return new Transaction
            {
                Direction = currentPosition.Direction.ToString(),
                EntryDate = currentPosition.Created,
                EntryLevel = currentPosition.EntryLevel,
                ExitLevel = exitLevel,
                ExitDate = exitDate,
                Size = currentPosition.Size,
                Risk = currentPosition.Stop.HasValue ? (currentPosition.EntryLevel - currentPosition.Stop.Value) * currentPosition.Size : 0,
                Reward = currentPosition.Limit.HasValue ? (currentPosition.Limit.Value - currentPosition.EntryLevel) * currentPosition.Size : 0,
                Gain = (exitLevel - currentPosition.EntryLevel) * currentPosition.Size,
                Ticker = ticker,
                ExchangeRate = exchangeRate,
                Position = currentPosition
            };
        }

        private Transaction CreateShortTransaction(Position currentPosition, DateTime exitDate, decimal exitLevel, string ticker, decimal exchangeRate)
        {
            return new Transaction
            {
                Direction = currentPosition.Direction.ToString(),
                EntryDate = currentPosition.Created,
                EntryLevel = currentPosition.EntryLevel,
                ExitLevel = exitLevel,
                ExitDate = exitDate,
                Size = currentPosition.Size,
                Risk = currentPosition.Stop.HasValue ? (currentPosition.Stop.Value - currentPosition.EntryLevel) * currentPosition.Size : 0,
                Reward = currentPosition.Limit.HasValue ? (currentPosition.EntryLevel - currentPosition.Limit.Value) * currentPosition.Size : 0,
                Gain = (currentPosition.EntryLevel - exitLevel) * currentPosition.Size,
                Ticker = ticker,
                ExchangeRate = exchangeRate,
                Position = currentPosition
            };
        }
    }
}
