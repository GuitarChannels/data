using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
	public class GuitarTermRepository : IGuitarTermRepository
	{
		private readonly IMongoCollection<GuitarTerm> _col;

		public GuitarTermRepository(IMongoCollection<GuitarTerm> col)
		{
			_col = col ?? throw new ArgumentNullException(nameof(col));
		}

		public async Task<IReadOnlyCollection<GuitarTerm>> GetAll()
		{
			return await _col
				.Find(new BsonDocument())
				.ToListAsync();
		}
	}
}
