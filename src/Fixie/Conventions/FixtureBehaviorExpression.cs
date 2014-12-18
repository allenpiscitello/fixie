﻿using System;
using Fixie.Internal;

namespace Fixie.Conventions
{
    public class FixtureBehaviorExpression
    {
        readonly Configuration config;

        internal FixtureBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Wraps each test fixture (test class instance) with the specified behavior. The
        /// behavior may perform custom actions before and/or after each test fixture executes.
        /// </summary>
        public FixtureBehaviorExpression Wrap<TFixtureBehavior>() where TFixtureBehavior : FixtureBehavior
        {
            config.WrapFixtures(() => (FixtureBehavior)Activator.CreateInstance(typeof(TFixtureBehavior)));
            return this;
        }

        /// <summary>
        /// Wraps each test fixture (test class instance) with the specified behavior. The
        /// behavior may perform custom actions before and/or after each test fixture executes.
        /// </summary>
        public FixtureBehaviorExpression Wrap(FixtureBehavior behavior)
        {
            config.WrapFixtures(() => behavior);
            return this;
        }

        /// <summary>
        /// Wraps each test fixture (test class instance) with the specified behavior. The
        /// behavior may perform custom actions before and/or after each test fixture executes.
        /// </summary>
        public FixtureBehaviorExpression Wrap(FixtureBehaviorAction behavior)
        {
            config.WrapFixtures(() => new LambdaFixtureBehavior(behavior));
            return this;
        }

        class LambdaFixtureBehavior : FixtureBehavior
        {
            readonly FixtureBehaviorAction execute;

            public LambdaFixtureBehavior(FixtureBehaviorAction execute)
            {
                this.execute = execute;
            }

            public void Execute(Fixture context, Action next)
            {
                execute(context, next);
            }
        }
    }
}