﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System.Collections.Generic;
using SME.SGP.Infra.Interface;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioPendenciaRegistroIndividual : RepositorioBase<PendenciaRegistroIndividual>, IRepositorioPendenciaRegistroIndividual
    {
		private const string SelectCompletoPadrao = @"SELECT 
	                                pri.id, 
	                                pri.alterado_por, 
	                                pri.alterado_rf, 
	                                pri.alterado_em, 
	                                pri.criado_por, 
	                                pri.criado_rf, 
	                                pri.criado_em, 
	                                pri.pendencia_id, 
	                                pri.turma_id,
	                                pria.id,
	                                pria.codigo_aluno,
	                                pria.pendencia_registro_individual_id,
									pria.situacao,
									p.id, 
									p.titulo, 
									p.descricao, 
									p.situacao, 
									p.tipo, 
									p.excluido, 
									p.migrado, 
									p.criado_em, 
									p.criado_por, 
									p.alterado_em, 
									p.alterado_por, 
									p.criado_rf, 
									p.alterado_rf, 
									p.instrucao
                                FROM 
	                                public.pendencia_registro_individual pri 
                                inner join
	                                public.pendencia p 
	                                on pri.pendencia_id = p.id
                                left join
	                                public.pendencia_registro_individual_aluno pria 
	                                on pri.id = pria.pendencia_registro_individual_id ";

        public RepositorioPendenciaRegistroIndividual(ISgpContext database, IServicoAuditoria servicoAuditoria) : base(database, servicoAuditoria)
        { }

		public async Task<PendenciaRegistroIndividual> ObterPendenciaRegistroIndividualPorPendenciaESituacao(long pendenciaId, SituacaoPendencia situacaoPendencia, 
			SituacaoPendenciaRegistroIndividualAluno situacaoAluno)
        {
			var sql = $@"{SelectCompletoPadrao}
                        where
	                        pri.pendencia_id = @pendenciaId
	                        and p.situacao = @situacao
							and pria.situacao = @situacaoAluno
	                        and not p.excluido";

            PendenciaRegistroIndividual resultado = null;
            await database.Conexao.QueryAsync<PendenciaRegistroIndividual, PendenciaRegistroIndividualAluno, Pendencia, PendenciaRegistroIndividual>(sql,
                (pendenciaRegistroIndividual, pendenciaRegistroIndividualAluno, pendencia) =>
                {
                    if (resultado is null)
                    {
                        resultado = pendenciaRegistroIndividual;
                        resultado.Pendencia = pendencia;
                    }

                    resultado.Alunos = resultado.Alunos ?? new List<PendenciaRegistroIndividualAluno>();
                    resultado.Alunos.Add(pendenciaRegistroIndividualAluno);
                    return resultado;
                },
                new { pendenciaId, situacao = (short)situacaoPendencia, situacaoAluno = (short)situacaoAluno });

            return resultado;
        }


		public async Task<PendenciaRegistroIndividual> ObterPendenciaRegistroIndividualPorTurmaESituacao(long turmaId, SituacaoPendencia situacaoPendencia)
        {
            var sql = $@"{SelectCompletoPadrao}
                        where
	                        pri.turma_id = @turmaId
	                        and p.situacao = @situacao
	                        and not p.excluido";

            PendenciaRegistroIndividual resultado = null;
            await database.Conexao.QueryAsync<PendenciaRegistroIndividual, PendenciaRegistroIndividualAluno, Pendencia, PendenciaRegistroIndividual>(sql,
                (pendenciaRegistroIndividual, pendenciaRegistroIndividualAluno, pendencia) =>
                {
                    if (resultado is null)
                    {
                        resultado = pendenciaRegistroIndividual;
						resultado.Pendencia = pendencia;
                    }

                    resultado.Alunos = resultado.Alunos ?? new List<PendenciaRegistroIndividualAluno>();
                    resultado.Alunos.Add(pendenciaRegistroIndividualAluno);
                    return resultado;
                },
                new { turmaId, situacao = (short)situacaoPendencia });

            return resultado;
        }

        public async Task<IEnumerable<long>> ObterAlunosCodigosComPendenciaAtivosDaTurmaAsync(long turmaId)
        {
            const string sql = @"
                                select
	                                pria.codigo_aluno
                                from
	                                pendencia_registro_individual_aluno pria
                                inner join pendencia_registro_individual pri on
	                                pria.pendencia_registro_individual_id = pri.id
                                where
	                                pria.situacao = 1
                                and pri.turma_id = @turmaId";


            return await database.Conexao.QueryAsync<long>(sql, new { turmaId });
        }

        public async Task<IEnumerable<long>> ObterIdsPendencias(int anoLetivo, string codigoUE)
        {
            var tipoPendencia = (int)TipoPendencia.AusenciaDeRegistroIndividual;
            var situacao = new List<int>() { (int)SituacaoPendencia.Pendente, (int)SituacaoPendencia.Resolvida };
            var query = @$"select distinct p.id 
                            from pendencia p
                            inner join pendencia_registro_individual pri on pri.pendencia_id = p.id 
                            inner join turma t on t.id = pri.turma_id 
                            inner join aula a on a.turma_id = t.turma_id 
                            inner join tipo_calendario tc on tc.id = a.tipo_calendario_id 
                            where tipo = @tipoPendencia
                                    and not p.excluido 
                                    and p.situacao = any(@situacao)
                                    and tc.ano_letivo = @anoLetivo 
                                    and a.ue_id = @codigoUE";

            return await database.Conexao.QueryAsync<long>(query, new
            {
                tipoPendencia,
                situacao = situacao.ToArray(),
                anoLetivo,
                codigoUE
            });
        }
    }
}