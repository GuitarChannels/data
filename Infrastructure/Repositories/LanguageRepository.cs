﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public sealed class LanguageRepository : ILanguageRepository
    {
        private readonly IMongoCollection<Channel> _col;

        public LanguageRepository(IMongoCollection<Channel> col)
        {
            _col = col ?? throw new ArgumentNullException(nameof(col));
        }

        #region Public Methods

        /// <summary>
        /// Gets a list of all available languages
        /// </summary>
        /// <returns></returns>
        public async Task<List<Language>> GetAll()
        {
            // fetch distinct languges from database
            var langCodes = await _col
                .DistinctAsync<string>("language", new BsonDocument())
                .Result
                .ToListAsync();

            // convert to language object
            return langCodes
                .Select(l => new Language(l))
                .ToList();
        }

        #endregion
    }
}
