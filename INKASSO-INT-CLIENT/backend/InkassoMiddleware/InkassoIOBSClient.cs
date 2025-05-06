using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using InkassoMiddleware.Models;

namespace InkassoMiddleware.IOBS
{
    [ServiceContract]
    public interface IIcelandicOnlineBankingClaimsSoap
    {
        [OperationContract]
        string SubmitClaim(string claimData);

        [OperationContract]
        Task<ClaimsQueryResult> QueryClaimsAsync(ClaimsQuery query);
    }

    public class InkassoIOBSClient : ClientBase<IIcelandicOnlineBankingClaimsSoap>, IIcelandicOnlineBankingClaimsSoap
    {
        public InkassoIOBSClient() : base(GetBinding(), GetEndpointAddress())
        {
            ClientCredentials.UserName.UserName = "servicetest";
            ClientCredentials.UserName.Password = "znvwYV5";
        }

        private static Binding GetBinding()
        {
            var binding = new WSHttpBinding
            {
                Security = new WSHttpSecurity
                {
                    Mode = SecurityMode.TransportWithMessageCredential,
                    Message = new NonDualMessageSecurityOverHttp
                    {
                        ClientCredentialType = MessageCredentialType.UserName
                    }
                },
                MaxReceivedMessageSize = 65536
            };
            return binding;
        }

        private static EndpointAddress GetEndpointAddress()
        {
            return new EndpointAddress("https://demo.inkasso.is/SOAP/IOBS/IcelandicOnlineBankingClaimsSoap.svc");
        }

        public string SubmitClaim(string claimData)
        {
            return Channel.SubmitClaim(claimData);
        }

        public async Task<ClaimsQueryResult> QueryClaimsAsync(ClaimsQuery query)
        {
            return await Channel.QueryClaimsAsync(query);
        }
    }
} 