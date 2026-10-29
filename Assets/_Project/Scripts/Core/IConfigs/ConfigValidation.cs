using System;
using System.Collections.Generic;
using System.Text;

namespace Asteroids.Core.IConfigs
{
    public static class ConfigValidation
    {
        /// <summary>Throws one exception that lists every problem, so a designer can fix them in one pass.</summary>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="problems"/> is not empty.</exception>
        public static void ThrowIfInvalid(string subject, IReadOnlyList<string> problems)
        {
            if (subject == null) throw new ArgumentNullException(nameof(subject));
            if (problems == null) throw new ArgumentNullException(nameof(problems));
            if (problems.Count == 0) return;

            var message = new StringBuilder();
            message.Append(subject).Append(" is invalid (").Append(problems.Count).Append(" problem(s)):");
            foreach (var problem in problems) message.AppendLine().Append("- ").Append(problem);

            throw new InvalidOperationException(message.ToString());
        }
    }
}
