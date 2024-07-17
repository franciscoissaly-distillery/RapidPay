namespace RapidPay.Framework.Api.Mapping
{


    public abstract class ModelMapper : IModelMapper
    {
        public ModelMapper() { }

        protected ModelMapper(MappingConfig config) 
        {
            ArgumentNullException.ThrowIfNull(config);
            Configuration = config;
        }


        public IList<TTarget> MapTo<TTarget, TSource>(IEnumerable<TSource> source)
        {
            return MapTo(NewTarget<TTarget, TSource>, source);
        }

        public IList<TTarget> MapTo<TTarget, TSource>(Func<TSource, TTarget> factory, IEnumerable<TSource> source)
        {
            return MapTo(factory, new List<TTarget>(), source);
        }


        public IList<TTarget> MapTo<TTarget, TSource>(IList<TTarget> target, IEnumerable<TSource> source)
        {
            return MapTo(NewTarget<TTarget, TSource>, new List<TTarget>(), source);
        }


        public IList<TTarget> MapTo<TTarget, TSource>(Func<TSource, TTarget> factory, IList<TTarget> target, IEnumerable<TSource> source)
        {
            if (target is not null)
                if (source is not null)
                    foreach (TSource item in source)
                        target.Add(MapTo(factory, item));
            return target;
        }

        public TTarget MapTo<TTarget, TSource>(TSource source)
        {
            return MapTo(NewTarget<TTarget, TSource>, source);
        }

        public TTarget MapTo<TTarget, TSource>(Func<TSource, TTarget> factory, TSource source)
        {
            ArgumentNullException.ThrowIfNull(factory);
            return MapTo(factory.Invoke(source), source);
        }

        public TTarget MapTo<TTarget, TSource>(TTarget target, TSource source)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(source);

            if (!Configuration.IsConfigured)
                OnConfigure(Configuration);

            var mapper = Configuration.GetMapper<TTarget, TSource>();
            if (mapper is null)
                throw new InvalidOperationException($"No factory method for creating instances of type '{typeof(TTarget).FullName}'");

            return mapper.Method.Invoke(target, source);
        }

        protected TTarget NewTarget<TTarget, TSource>(TSource source)
        {
            if (!Configuration.IsConfigured)
                OnConfigure(Configuration);

            var factory = Configuration.GetFactory<TTarget, TSource>();
            if (factory is not null)
                return factory.Method.Invoke(source);

            var noSourceFactory = Configuration.GetFactory<TTarget, object>();
            if (noSourceFactory is not null)
                return noSourceFactory.Method.Invoke(source!);

            if (typeof(TTarget).GetConstructor(Type.EmptyTypes) is not null)
                return Activator.CreateInstance<TTarget>();

            throw new InvalidOperationException($"No factory method for creating instances of type '{typeof(TTarget).FullName}'");
        }

        protected abstract void OnConfigure(MappingConfig config);

        protected MappingConfig Configuration { get; } = new MappingConfig();

        protected class MappingConfig
        {
            protected List<IMethodWrapper> Mappers { get; } = new();
            protected List<IMethodWrapper> Factories { get; } = new();
            public bool IsConfigured {get; private set;}

            public Mapper<TTarget, TSource>? GetMapper<TTarget, TSource>()
            {
                var selected = (from candidate in Mappers
                                where candidate.TargetType == typeof(TTarget)
                                && candidate.SourceType == typeof(TSource)
                                select candidate).FirstOrDefault();

                return selected as Mapper<TTarget, TSource>;
            }

            public void AddMapping<TTarget, TSource>(Action<TTarget, TSource> mapperMethod)
            {
                ArgumentNullException.ThrowIfNull(mapperMethod);
                RemoveMapping<TTarget, TSource>();
                Mappers.Add(new Mapper<TTarget, TSource>(mapperMethod));
                IsConfigured = true;
            }

            public void AddMapping<TTarget, TSource>(Func<TTarget, TSource, TTarget> mapperMethod)
            {
                ArgumentNullException.ThrowIfNull(mapperMethod);
                RemoveMapping<TTarget, TSource>();
                Mappers.Add(new Mapper<TTarget, TSource>(mapperMethod));
                IsConfigured = true;
            }

            public void RemoveMapping<TTarget, TSource>()
            {
                var current = GetMapper<TTarget, TSource>();
                if (current is not null)
                    Mappers.Remove(current);
            }


            public Factory<TTarget, TSource>? GetFactory<TTarget, TSource>()
            {
                var targetCandidates = from candidate in Factories
                                       where candidate.TargetType == typeof(TTarget)
                                       select candidate;

                var selected = (from candidate in targetCandidates
                                where candidate.SourceType == typeof(TSource)
                                select candidate).FirstOrDefault();

                return selected as Factory<TTarget, TSource>;
            }

            public void AddFactory<TTarget, TSource>(Func<TSource, TTarget> factoryMethod)
            {
                ArgumentNullException.ThrowIfNull(factoryMethod);
                RemoveFactory<TTarget, TSource>();
                Factories.Add(new Factory<TTarget, TSource>(factoryMethod));
                IsConfigured = true;
            }
            public void RemoveFactory<TTarget, TSource>()
            {
                var current = GetFactory<TTarget, TSource>();
                if (current is not null)
                    Factories.Remove(current);
            }

            public void AddFactory<TTarget>(Func<TTarget> factoryMethod)
            {
                ArgumentNullException.ThrowIfNull(factoryMethod);
                RemoveFactory<TTarget>();
                Factories.Add(new Factory<TTarget>(factoryMethod));
                IsConfigured = true;
            }

            public void RemoveFactory<TTarget>()
            {
                RemoveFactory<TTarget, object>();
            }
        }

        protected interface IMethodWrapper
        {
            Type TargetType { get; }
            Type SourceType { get; }
        }

        protected abstract class MethodWrapper<TTarget, TSource> : IMethodWrapper
        {
            public Type TargetType => typeof(TTarget);
            public Type SourceType => typeof(TSource);
        }

        protected class Factory<TTarget, TSource> : MethodWrapper<TTarget, TSource>
        {
            public Func<TSource, TTarget> Method { get; }

            public Factory(Func<TSource, TTarget> method)
            {
                ArgumentNullException.ThrowIfNull(method);
                Method = method; ;
            }
            public Factory(Func<TTarget> method)
            {
                ArgumentNullException.ThrowIfNull(method);
                Method = (_) => method.Invoke();
            }
        }

        protected class Factory<TTarget> : Factory<TTarget, object>
        {
            public Factory(Func<TTarget> method) : base(method)
            { }
        }

        protected class Mapper<TTarget, TSource> : MethodWrapper<TTarget, TSource>
        {
            public Func<TTarget, TSource, TTarget> Method { get; }

            public Mapper(Func<TTarget, TSource, TTarget> method)
            {
                ArgumentNullException.ThrowIfNull(method);
                Method = method; ;
            }

            public Mapper(Action<TTarget, TSource> method)
            {
                ArgumentNullException.ThrowIfNull(method);
                Method = (target, source) =>
                {
                    method.Invoke(target, source);
                    return target;
                };
            }
        }
    }
}
