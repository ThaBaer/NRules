﻿using System;
using System.Linq.Expressions;
using NRules.Rete;
using NRules.Utilities;

namespace NRules.AgendaFilters
{
    internal interface IActivationCondition
    {
        bool Invoke(Activation activation);
    }

    internal class ActivationCondition : IActivationCondition
    {
        private readonly LambdaExpression _expression;
        private readonly FastDelegate<Func<object[], bool>> _compiledExpression;
        private readonly IndexMap _tupleFactMap;

        public ActivationCondition(LambdaExpression expression, FastDelegate<Func<object[], bool>> compiledExpression, IndexMap tupleFactMap)
        {
            _expression = expression;
            _compiledExpression = compiledExpression;
            _tupleFactMap = tupleFactMap;
        }

        public bool Invoke(Activation activation)
        {
            var tuple = activation.Tuple;
            var activationFactMap = activation.FactMap;

            var args = new object[_compiledExpression.ArrayArgumentCount];

            int index = tuple.Count - 1;
            foreach (var fact in tuple.Facts)
            {
                var mappedIndex = _tupleFactMap[activationFactMap[index]];
                IndexMap.SetElementAt(args, mappedIndex, fact.Object);
                index--;
            }

            try
            {
                var result = _compiledExpression.Delegate.Invoke(args);
                return result;
            }
            catch (Exception e)
            {
                throw new ActivationExpressionException(e, _expression, activation);
            }
        }
    }
}
