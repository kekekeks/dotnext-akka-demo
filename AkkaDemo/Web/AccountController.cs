using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AkkaDemo.Actors;

namespace AkkaDemo.Web
{
    [RoutePrefix("accounts")]
    public class AccountController : ApiController
    {
        private readonly AccountCommander _accounts;

        public AccountController(AccountCommander accounts)
        {
            _accounts = accounts;
        }

        [Route(""), HttpPost]
        public object CreateAccount()
        {
            return new {Id = Guid.NewGuid()};
        }

        [Route("{id}"), HttpGet]
        public Task<AccountActor.CurrentBalanceQueryResult> GetFunds(Guid id)
        {
            return _accounts.Ask(id, new AccountActor.CurrentBalanceQuery());
        }

        public class FundsModel
        {
             public decimal Amount { get; set; }
        }

        [Route("{id}/deposit"), HttpPost]
        public Task<GenericRequestResult> DepositFunds(Guid id, [FromBody]FundsModel request)
        {
            return _accounts.Ask(id, new AccountActor.DepositCommand(request.Amount));
        }

        [Route("{id}/withdraw"), HttpPost]
        public async Task<GenericRequestResult> WithdrawFunds(Guid id, [FromBody]FundsModel request)
        {
            return await _accounts.Ask(id, new AccountActor.DepositCommand(request.Amount));
        }


        [Route("{id}/block"), HttpPost]
        public async Task<AccountActor.BlockFundsCommandResult> BlockFunds(Guid id, [FromBody]FundsModel request)
        {
            return await _accounts.Ask(id, new AccountActor.BlockFundsCommand(request.Amount));
        }

        public class TransactionPatchModel
        {
            public bool Complete { get; set; }
        }

        [Route("{id}/transactions/{transactionId}"), HttpPatch]
        public async Task<GenericRequestResult> CompleteTransaction(Guid id, Guid transactionId, [FromBody]TransactionPatchModel request)
        {
            return await _accounts.Ask(id, request.Complete
                ? (GenericRequest) new AccountActor.CompleteTransaction(transactionId)
                : new AccountActor.CancelTransaction(transactionId));
        }


    }
}
