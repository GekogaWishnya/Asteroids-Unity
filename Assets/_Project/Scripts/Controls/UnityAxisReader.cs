using System;
using UnityEngine;

namespace Asteroids.Controls
{
    /// <summary><see cref="IAxisReader"/> over the legacy <see cref="Input"/> class.</summary>
    public sealed class UnityAxisReader : IAxisReader
    {
        public float Read(string axis)
        {
            try
            {
                return Input.GetAxisRaw(axis);
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException(
                    $"Input axis '{axis}' is not defined. Add it in Project Settings > Input Manager.", e);
            }
        }
    }
}