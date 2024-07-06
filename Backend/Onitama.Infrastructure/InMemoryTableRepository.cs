using Onitama.Core;
using Onitama.Core.TableAggregate.Contracts;
using Onitama.Core.Util;
using Onitama.Infrastructure.Util;
using System.Diagnostics;

namespace Onitama.Infrastructure;

/// <inheritdoc cref="ITableRepository"/>
internal class InMemoryTableRepository : ITableRepository
{
    private readonly ExpiringDictionary<Guid, ITable> _tableDictionary;

    public InMemoryTableRepository()
    {
        _tableDictionary = new ExpiringDictionary<Guid, ITable>(TimeSpan.FromMinutes(15));
    }

    public void Add(ITable table)
    {
        _tableDictionary.AddOrReplace(table.Id, table);
    }

    public ITable Get(Guid tableId)
    {
        if (_tableDictionary.TryGetValue(tableId, out ITable table))
        {
            return table!;
        }
        throw new DataNotFoundException();
    }

    public void Remove(Guid tableId)
    {
        _tableDictionary.TryRemove(tableId, out ITable _);
    }

    public IList<ITable> FindTablesWithAvailableSeats()
    {

        IList<ITable> tablesWithAvailableSeats = new List<ITable>();
        foreach(ITable item in _tableDictionary.Values)
        {
            if (item.HasAvailableSeat == true)
            {
                tablesWithAvailableSeats.Add(item);
            }
        }
        return tablesWithAvailableSeats;
    }
}