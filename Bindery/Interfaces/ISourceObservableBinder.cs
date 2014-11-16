﻿using System;
using System.ComponentModel;
using Bindery.Implementations;
using Bindery.Interfaces.Observables;

namespace Bindery.Interfaces
{
    public interface ISourceObservableBinder<TSource, TArg> 
        where TSource : INotifyPropertyChanged
    {
        IOnNextDefined<TSource> OnNext(Action<TArg> onNext);
    }
}