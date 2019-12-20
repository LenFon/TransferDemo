﻿using NEventStore;
using NEventStore.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Len.Domain.Persistence.Memento
{
    public class MementoStore : IMementoStore
    {
        private readonly IPersistStreams _persistStreams;

        public MementoStore(IPersistStreams persistStreams)
        {
            _persistStreams = persistStreams ?? throw new ArgumentNullException(nameof(persistStreams));
        }

        public Task<IMemento> GetMementoAsync(Guid aggregateId, int version = int.MaxValue)
        {
            var snapshot = _persistStreams.GetSnapshot(Bucket.Default, aggregateId, version);

            IMemento memento;
            if (snapshot != null)
            {
                memento = snapshot.Payload as IMemento;
            }
            else
            {
                memento = null;
            }

            return Task.FromResult(memento);
        }
    }
}
