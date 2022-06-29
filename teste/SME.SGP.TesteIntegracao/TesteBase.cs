using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Aplicacao;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace SME.SGP.TesteIntegracao
{
    [Collection("TesteIntegradoSGP")]
    public class TesteBase : IClassFixture<TestFixture>
    {
        private readonly CollectionFixture _collectionFixture;

        public ServiceProvider ServiceProvider {  get {  return _collectionFixture.ServiceProvider; } }

        public TesteBase(CollectionFixture collectionFixture)
        {
            _collectionFixture = collectionFixture;
            _collectionFixture.Database.LimparBase();

            RegistrarFakes(_collectionFixture.services);
            _collectionFixture.BuildServiceProvider();
        }

        protected virtual void RegistrarFakes(IServiceCollection services)
        {
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<PublicarFilaSgpCommand, bool>),
                typeof(PublicarFilaSgpCommandHandlerFake), ServiceLifetime.Scoped));
        }

        public Task InserirNaBase<T>(IEnumerable<T> objetos) where T : class, new()
        {
            _collectionFixture.Database.Inserir(objetos);
            return Task.CompletedTask;
        }

        public Task InserirNaBase<T>(T objeto) where T : class, new()
        {
            _collectionFixture.Database.Inserir(objeto);
            return Task.CompletedTask;
        }

        public Task InserirNaBase(string nomeTabela, params string[] campos)
        {
            _collectionFixture.Database.Inserir(nomeTabela, campos);
            return Task.CompletedTask;
        }

        public List<T> ObterTodos<T>() where T : class, new()
        {
            return _collectionFixture.Database.ObterTodos<T>();
        }

        public T ObterPorId<T, K>(K id)
            where T : class, new()
            where K : struct
        {
            return _collectionFixture.Database.ObterPorId<T, K>(id);
        }
    }
}