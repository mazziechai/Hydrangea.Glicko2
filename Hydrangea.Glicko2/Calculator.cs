// Copyright (C) 2021 mazziechai
// 
// This file is part of Dongurigaeru.
// 
// Dongurigaeru is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Dongurigaeru is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Dongurigaeru.  If not, see <http://www.gnu.org/licenses/>.

using Hydrangea.Glicko2.Interfaces;

namespace Hydrangea.Glicko2
{
    /// <summary>
    /// Calculates new ratings.
    /// </summary>
    public class Calculator : ICalculator
    {
        public double StandardRating { get; init; } = 1500;
        public double VolatilityConstraint { get; init; } = 0.75;
        public double ConvergenceTolerance { get; init; } = 0.000001;
    }
}
