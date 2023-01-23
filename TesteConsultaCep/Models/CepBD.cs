﻿using Microsoft.EntityFrameworkCore;

namespace TesteConsultaCep.Models
{
    public class CepBD : DbContext
    {
        public int Id { get; set; }
        public string CEP { get; set; }
        public string Logradouro { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Localidade { get; set; }
        public string Uf { get; set; }
        public int unidade { get; set; }
        public int Ibge { get; set; }
        public string Gia { get; set; }
    }
}
