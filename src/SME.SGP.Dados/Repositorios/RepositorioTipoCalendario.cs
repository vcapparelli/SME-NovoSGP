﻿using Dapper;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using SME.SGP.Infra.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioTipoCalendario : RepositorioBase<TipoCalendario>, IRepositorioTipoCalendario
    {
        public RepositorioTipoCalendario(ISgpContext conexao, IServicoAuditoria servicoAuditoria) : base(conexao, servicoAuditoria)
        {
        }
        public async Task<PeriodoEscolar> ObterPeriodoEscolarPorCalendarioEData(long tipoCalendarioId, DateTime dataParaVerificar)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("select pe.*, tc.* from periodo_escolar pe");
            query.AppendLine("inner join tipo_calendario tc");
            query.AppendLine("on tc.id = pe.tipo_calendario_id");
            query.AppendLine("where tc.id = @tipoCalendarioId");
            query.AppendLine("and @dataParaVerificar between symmetric pe.periodo_inicio::date and pe.periodo_fim ::date");

            return (await database.Conexao.QueryAsync<PeriodoEscolar, TipoCalendario, PeriodoEscolar>(query.ToString(), (pe, tc) =>
            {
                pe.AdicionarTipoCalendario(tc);
                return pe;

            }, new { tipoCalendarioId, dataParaVerificar }, splitOn: "id")).FirstOrDefault();
        }
        public async Task<IEnumerable<TipoCalendario>> BuscarPorAnoLetivo(int anoLetivo)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("select *");
            query.AppendLine("from tipo_calendario");
            query.AppendLine("where excluido = false");
            query.AppendLine("and ano_letivo = @anoLetivo");

            return await database.Conexao.QueryAsync<TipoCalendario>(query.ToString(), new { anoLetivo });
        }

        public async Task<TipoCalendario> BuscarPorAnoLetivoEModalidade(int anoLetivo, ModalidadeTipoCalendario modalidade, int semestre = 0)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("select *");
            query.AppendLine("from tipo_calendario t");
            query.AppendLine("where t.excluido = false");
            query.AppendLine("and t.ano_letivo = @anoLetivo");
            query.AppendLine("and t.modalidade = @modalidade");

            DateTime dataReferencia = DateTime.MinValue;
            if (modalidade == ModalidadeTipoCalendario.EJA)
            {
                var periodoReferencia = semestre == 1 ? "periodo_inicio < @dataReferencia" : "periodo_fim > @dataReferencia";
                query.AppendLine($"and exists(select 0 from periodo_escolar p where tipo_calendario_id = t.id and {periodoReferencia})");

                dataReferencia = new DateTime(anoLetivo, semestre == 1 ? 6 : 8, 1);
            }

            return await database.Conexao.QueryFirstOrDefaultAsync<TipoCalendario>(query.ToString(), new { anoLetivo, modalidade = (int)modalidade, dataReferencia });
        }

        public async Task<IEnumerable<TipoCalendario>> BuscarPorAnoLetivoEModalidade(int anoLetivo, ModalidadeTipoCalendario modalidade, DateTime dataReferencia)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("select *");
            query.AppendLine("from tipo_calendario t");
            query.AppendLine("where t.excluido = false");
            query.AppendLine("and t.ano_letivo = @anoLetivo");
            query.AppendLine("and t.modalidade = @modalidade");

            if (modalidade == ModalidadeTipoCalendario.EJA)
            {
                query.AppendLine($"and exists(select 0 from periodo_escolar p where tipo_calendario_id = t.id and @dataReferencia BETWEEN p.periodo_inicio and p.periodo_fim)");
            }

            return await database.Conexao.QueryAsync<TipoCalendario>(query.ToString(), new { anoLetivo, modalidade = (int)modalidade, dataReferencia });
        }

        public async Task<IEnumerable<TipoCalendario>> ListarPorAnoLetivo(int anoLetivo)
        {
            StringBuilder query = ObterQueryListarPorAnoLetivo();

            return await database.Conexao.QueryAsync<TipoCalendario>(query.ToString(), new { anoLetivo });
        }

        public override TipoCalendario ObterPorId(long id)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("select * ");
            query.AppendLine("from tipo_calendario ");
            query.AppendLine("where excluido = false ");
            query.AppendLine("and id = @id ");

            return database.Conexao.QueryFirstOrDefault<TipoCalendario>(query.ToString(), new { id });
        }

        public async Task<IEnumerable<TipoCalendario>> ObterTiposCalendario()
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("select");
            query.AppendLine("id,");
            query.AppendLine("nome,");
            query.AppendLine("ano_letivo,");
            query.AppendLine("modalidade,");
            query.AppendLine("periodo");
            query.AppendLine("from tipo_calendario");
            query.AppendLine("where excluido = false");

            return await database.Conexao.QueryAsync<TipoCalendario>(query.ToString());
        }

        public async Task<bool> VerificarRegistroExistente(long id, string nome)
        {
            StringBuilder query = new StringBuilder();

            var nomeMaiusculo = nome.ToUpper().Trim();
            query.AppendLine("select count(*) ");
            query.AppendLine("from tipo_calendario ");
            query.AppendLine("where upper(nome) = @nomeMaiusculo ");
            query.AppendLine("and excluido = false");

            if (id > 0)
                query.AppendLine("and id <> @id");

            int quantidadeRegistrosExistentes = await database.Conexao.QueryFirstAsync<int>(query.ToString(), new { id, nomeMaiusculo });

            return quantidadeRegistrosExistentes > 0;
        }

        private static StringBuilder ObterQueryListarPorAnoLetivo()
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("select *");
            query.AppendLine("from tipo_calendario");
            query.AppendLine("where not excluido");
            query.AppendLine("and ano_letivo = @anoLetivo");
            return query;
        }

        public async Task<bool> PeriodoEmAberto(long tipoCalendarioId, DateTime dataReferencia, int bimestre = 0, bool ehAnoLetivo = false)
        {
            var query = new StringBuilder(@"select count(pe.Id)
                          from periodo_escolar pe 
                         where pe.tipo_calendario_id = @tipoCalendarioId
                           and periodo_fim::date >= @dataReferencia::date ");

            if (!ehAnoLetivo)
            {
                query.AppendLine("and periodo_inicio <= @dataReferencia");
            }

            if (bimestre > 0)
                query.AppendLine(" and pe.bimestre = @bimestre");

            return await database.Conexao.QueryFirstAsync<int>(query.ToString(), new { tipoCalendarioId, dataReferencia, bimestre }) > 0;
        }

        public async Task<long> ObterIdPorAnoLetivoEModalidadeAsync(int anoLetivo, ModalidadeTipoCalendario modalidade, int semestre = 0)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("select id");
            query.AppendLine("from tipo_calendario t");
            query.AppendLine("where t.excluido = false");
            query.AppendLine("and t.ano_letivo = @anoLetivo");
            query.AppendLine("and t.modalidade = @modalidade");
            query.AppendLine("and t.situacao ");

            DateTime dataReferencia = DateTime.MinValue;
            if (modalidade == ModalidadeTipoCalendario.EJA)
            {
                var periodoReferencia = semestre == 1 ? "periodo_inicio < @dataReferencia" : "periodo_fim > @dataReferencia";
                query.AppendLine($"and exists(select 0 from periodo_escolar p where tipo_calendario_id = t.id and {periodoReferencia})");

                dataReferencia = new DateTime(anoLetivo, semestre == 1 ? 6 : 8, 1);
            }

            return await database.Conexao.QueryFirstOrDefaultAsync<long>(query.ToString(), new { anoLetivo, modalidade = (int)modalidade, dataReferencia });
        }

        public async Task<IEnumerable<TipoCalendario>> ListarPorAnoLetivoEModalidades(int anoLetivo, int[] modalidades, int semestre = 0)
        {
            StringBuilder query = ObterQueryListarPorAnoLetivo();
            query.AppendLine(" and modalidade = any(@modalidades) ");

            DateTime dataReferencia = DateTime.MinValue;
            if (semestre > 0)
            {
                var periodoReferencia = semestre == 1 ? "periodo_inicio < @dataReferencia" : "periodo_fim > @dataReferencia";
                query.AppendLine($" and exists(select 0 from periodo_escolar p where tipo_calendario_id = tipo_calendario.id and {periodoReferencia}) ");

                dataReferencia = new DateTime(anoLetivo, semestre == 1 ? 6 : 8, 1);
            }

            return await database.Conexao.QueryAsync<TipoCalendario>(query.ToString(), new { anoLetivo, modalidades, dataReferencia });
        }

        public async Task<IEnumerable<TipoCalendarioRetornoDto>> ListarPorAnoLetivoDescricaoEModalidades(int anoLetivo, string descricao, IEnumerable<int> modalidades)
        {
            var query = new StringBuilder(@"select tc.id,
   		                                           tc.ano_letivo as AnoLetivo,   		  
   		                                           tc.nome,
   		                                           tc.ano_letivo ||' - '|| tc.nome as descricao,
   		                                           tc.modalidade
                                              from tipo_calendario tc
                                             where not tc.excluido
                                               and tc.ano_letivo = @anoLetivo ");

            if (!string.IsNullOrEmpty(descricao))
                query.AppendLine("and UPPER(ano_letivo ||' - '|| nome) like UPPER('%{descricao}%') ");

            if (modalidades.Any() && !modalidades.Any(c => c == -99))
                query.AppendLine("and modalidade = any(@modalidades)");

            return await database.Conexao.QueryAsync<TipoCalendarioRetornoDto>(query.ToString(), new { anoLetivo, descricao, modalidades });
        }

        public async Task<IEnumerable<TipoCalendarioBuscaDto>> ObterTiposCalendarioPorDescricaoAsync(string descricao)
        {
            string query = $@"select id, 
	                                 ano_letivo,
	                                 nome,
                                     modalidade,
	                                 ano_letivo ||' - '|| nome as descricao,
                                     migrado,
                                     periodo,
                                     situacao
                                from tipo_calendario tc
                               where UPPER(ano_letivo ||' - '|| nome) like UPPER('%{descricao}%')
                                 and not excluido
                               order by descricao desc
                               limit 10";

            return await database.Conexao.QueryAsync<TipoCalendarioBuscaDto>(query.ToString());
        }

        public async Task<string> ObterNomePorId(long tipoCalendarioId)
        {
            var query = @"select nome from tipo_calendario where id = @tipoCalendarioId";

            return await database.Conexao.QueryFirstAsync<string>(query, new { tipoCalendarioId });
        }

        public async Task<IEnumerable<TipoCalendarioBuscaDto>> ListarPorAnosLetivoEModalidades(int[] anosLetivo, int[] modalidades, string nome)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("select *, ano_letivo ||' - '|| nome as descricao");
            query.AppendLine("from tipo_calendario");
            query.AppendLine("where not excluido");
            query.AppendLine("and ano_letivo = any(@anosLetivo)");
            query.AppendLine("and modalidade = any(@modalidades)");

            if (!string.IsNullOrEmpty(nome))
                query.Append($"and upper(f_unaccent(nome)) like UPPER('%{nome}%')");

            query.AppendLine("order by ano_letivo desc");

            return await database.Conexao.QueryAsync<TipoCalendarioBuscaDto>(query.ToString(), new { anosLetivo, modalidades, nome });
        }
        public async Task<IEnumerable<PeriodoCalendarioBimestrePorAnoLetivoModalidadeDto>> ObterPeriodoTipoCalendarioBimestreAsync(int anoLetivo, int modalidadeTipoCalendarioId, int semestre = 0)
        {
            var query = @"select
	                        pe.id as periodoEscolarId,
	                        pe.bimestre,
	                        pe.periodo_inicio as PeriodoInicio,
	                        pe.periodo_fim as PeriodoFim
                        from
	                        tipo_calendario tc
                        inner join periodo_escolar pe on
	                        pe.tipo_calendario_id = tc.id
                        where
	                        tc.ano_letivo = @anoLetivo
	                        and tc.modalidade = @modalidadeTipoCalendarioId
	                        and not tc.excluido ";
            var dataReferencia = new DateTime(anoLetivo, semestre == 1 ? 6 : 8, 1);

            if (modalidadeTipoCalendarioId == (int)Modalidade.EJA.ObterModalidadeTipoCalendario() && semestre > 0)
            {
                if (semestre == 1)
                    query += $"and pe.periodo_inicio < @dataReferencia";
                else query += $"and pe.periodo_fim > @dataReferencia";
            }

            return await database.Conexao.QueryAsync<PeriodoCalendarioBimestrePorAnoLetivoModalidadeDto>(query.ToString(), new { anoLetivo, modalidadeTipoCalendarioId, dataReferencia });
        }

        public async Task<long> ObterTipoCalendarioIdPorAnoLetivoModalidadeEDataReferencia(int anoLetivo, ModalidadeTipoCalendario modalidadeTipoCalendarioId, DateTime dataReferencia)
        {
            var query = @"select tc.id
                            from tipo_calendario tc 
                           inner join periodo_escolar pe on pe.tipo_calendario_id = tc.id 
                           where tc.modalidade = @modalidadeTipoCalendarioId
                             and tc.ano_letivo = @anoLetivo
                             and not tc.excluido 
                             and @dataReferencia::date between pe.periodo_inicio::date and pe.periodo_fim::date ";

            var parametros = new
            {
                anoLetivo,
                modalidadeTipoCalendarioId,
                dataReferencia
            };

            return await database.Conexao.QueryFirstOrDefaultAsync<long>(query, parametros);
        }

        public async Task<int> ObterAnoLetivoUltimoTipoCalendarioPorDataReferencia(int anoReferencia, ModalidadeTipoCalendario modalidadeTipoCalendario)
        {
            var sqlQuery = @"select ano_letivo
	                            from tipo_calendario tc 
                             where ano_letivo < @anoReferencia and
	                               modalidade = @modalidadeTipoCalendario
                             order by ano_letivo desc
                             limit 1;";

            return await database.Conexao
                .QueryFirstOrDefaultAsync<int>(sqlQuery, new
                {
                    anoReferencia,
                    modalidadeTipoCalendario
                });
        }
    }
}