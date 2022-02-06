using Microsoft.Extensions.Options;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InverseCurveSidebarBot
{
    public class CurveExchangeRateService
    {
        private readonly Web3 _web3;

        private BotSettings _botSettings;

        private readonly string? _poolAddress;
        private readonly int _i;
        private readonly int _j;

        private BigDecimal _adminFee;
        private BigDecimal _fee;

        public CurveExchangeRateService(IOptions<BotSettings> settings) 
        {
            _botSettings = settings.Value;
            _web3 = new Web3(_botSettings.Web3);
            _poolAddress = _botSettings.Pool;
            _i = _botSettings.I ?? 0;
            _j = _botSettings.J ?? 1;

            _ = CacheFees();
        }

        public async Task CacheFees()
        {
            var getAdminFeeMessage = new GetAdminFeeFunction();
            var adminFeeHandler = _web3.Eth.GetContractQueryHandler<GetAdminFeeFunction>();
            var adminFee = await adminFeeHandler.QueryAsync<BigInteger>(_poolAddress, getAdminFeeMessage);

            _adminFee = Web3.Convert.FromWeiToBigDecimal(adminFee, 10);

            var feeMessage = new GetFeeFunction();
            var feeHandler = _web3.Eth.GetContractQueryHandler<GetFeeFunction>();
            var fee = await feeHandler.QueryAsync<BigInteger>(_poolAddress, feeMessage);

            _fee = Web3.Convert.FromWeiToBigDecimal(fee, 10);
        }

        private async Task<BigInteger> _GetExchangeRateWithFees()
        {
            var getDyUnderlyingFunctionMessage = new GetDyUnderlyingFunction()
            {
                i = _i,
                j = _j,
                dx = BigInteger.Pow(10, 18)
            };

            var underylingDyHandler = _web3.Eth.GetContractQueryHandler<GetDyUnderlyingFunction>();
            var underylingDy = await underylingDyHandler.QueryAsync<BigInteger>(_poolAddress, getDyUnderlyingFunctionMessage);

            return underylingDy;
        }

        public async Task<decimal> GetExchangeRateWithFees()
        {
            var exchangeRateWithFees = await _GetExchangeRateWithFees();
            var underylingDyInEth = Web3.Convert.FromWei(exchangeRateWithFees);

            return underylingDyInEth;
        }

        public async Task<BigInteger> _GetExchangeRateWithoutFees()
        {
            var exchangeRateWithFees = await _GetExchangeRateWithFees();
            var underylingDyInEth = Web3.Convert.FromWei(exchangeRateWithFees);

            var denom = 1 - _fee - _fee * _adminFee;
            var underylingDyWithoutFees = underylingDyInEth / denom;

            var wei = Web3.Convert.ToWei(underylingDyWithoutFees);

            return wei;
        }

        public async Task<decimal> GetExchangeRateWithoutFees()
        {
            var exchangeRateWithoutFees = await _GetExchangeRateWithoutFees();
            var underylingDyWithoutFeesInEth = Web3.Convert.FromWei(exchangeRateWithoutFees);

            return underylingDyWithoutFeesInEth;
        }
    }
}
