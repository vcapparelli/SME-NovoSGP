﻿using MediatR;
using System;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class InserirComunicadoCommand : IRequest<bool>
    {
        public InserirComunicadoCommand(string titulo, string descricao, DateTime dataEnvio, DateTime? dataExpiracao, int anoLetivo, string codigoDre, string codigoUe, IEnumerable<string> turmas, bool alunosEspecificados, int[] modalidades, int semestre, IEnumerable<string> alunos, string seriesResumidas, long? tipoCalendarioId, long? eventoId)
        {
            Titulo = titulo;
            Descricao = descricao;
            DataEnvio = dataEnvio;
            DataExpiracao = dataExpiracao;
            AnoLetivo = anoLetivo;
            CodigoDre = codigoDre;
            CodigoUe = codigoUe;
            Turmas = turmas;
            AlunosEspecificados = alunosEspecificados;
            Modalidades = modalidades;
            Semestre = semestre;
            Alunos = alunos;
            SeriesResumidas = seriesResumidas;
            TipoCalendarioId = tipoCalendarioId;
            EventoId = eventoId;
        }

        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataEnvio { get; set; }
        public DateTime? DataExpiracao { get; set; }
        public int AnoLetivo { get; set; }
        public string CodigoDre { get; set; }
        public string CodigoUe { get; set; }
        public IEnumerable<string> Turmas { get; set; }
        public bool AlunosEspecificados { get; set; }
        public int[] Modalidades { get; set; }
        public int Semestre { get; set; }
        public IEnumerable<string> Alunos { get; set; }
        public string SeriesResumidas { get; set; }
        public long? TipoCalendarioId { get; set; }
        public long? EventoId { get; set; }
    }
}