// Copyright (C) 2021 mazziechai
// 
// Glicko-2 is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Glicko-2 is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with Glicko-2. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace Hydrangea.Glicko2.Interfaces
{
    /// <summary>
    /// Standards for a Glicko-2 calculator.
    /// </summary>
    public interface ICalculator
    {
        /// <summary>
        /// The rating value to use when converting it to the Glicko-2 scale.
        /// The Glicko-2 specifications sets this to 1500.
        /// </summary>
        public double StandardRating { get; }
        /// <summary>
        /// <para>
        /// The volatility constraint (τ) to use when calculating 
        /// a new volatility. Smaller values prevent the volatility from
        /// changing too much, which then prevents massive changes in ratings
        /// from an improbable result (as calculated by the system).
        /// </para>
        /// The recommended value is 0.3-1.2.
        /// </summary>
        public double VolatilityConstraint { get; }
        /// <summary>
        /// <para>
        /// The convergence tolerance (ε) to use when calculating a new
        /// volatility. This is used to restrict how many iterations we are
        /// performing in the algorithm to get as close to the limit as
        /// possible, since it would take an infinite amount to actually
        /// converge. This creates a margin of error that should hopefully
        /// be small enough while not taking too many iterations, which
        /// would lower the margin of error insignificantly.
        /// </para>
        /// The recommended value is 0.000001, which is sufficiently small
        /// enough to keep iterations as low as possible.
        /// </summary>
        public double ConvergenceTolerance { get; }

        /// <summary>
        /// Calculates a new rating from a list of IResults and returns it.
        /// </summary>
        public IRatingInfo Rate(IRatingInfo rating, IList<IResult> results)
        {
            // Setting the rating variables.
            var μ = (rating.Rating - StandardRating) / 173.7178;
            var φ = rating.RatingDeviation / 173.7178;
            var σ = rating.Volatility;
            // Initializing the variance.
            double v = 0;
            // Initializing Glicko-2 score.
            double s = 0;

            if (results.Count != 0)
            {
                foreach (var result in results)
                {
                    // Getting the opponent.
                    var opponent = result.Scores.Keys.Where(p => !p.Equals(rating)).Single();
                    // Setting the opponent's rating variables.
                    var μⱼ = (opponent.Rating - StandardRating) / 173.7178;
                    var φⱼ = opponent.RatingDeviation / 173.7178;
                    var score = result.Scores[rating];

                    // Compute the inverse of the variance.
                    v += V(μ, μⱼ, φⱼ);
                    // Compute the Glicko-2 score.
                    s += S(μ, μⱼ, φⱼ, score);
                }
                // Since we calculated the inverse, make it direct.
                v = Pow(v, -1);
                // The expected difference in rating.
                double Δ = v * s;
                // Calculate the new volatility.
                var σʹ = DetermineVolatility(φ, σ, Δ, v);
                // Calculate the new RD.
                var φ1 = Sqrt(Pow(φ, 2) + Pow(σʹ, 2));
                var φʹ = 1 / (Sqrt(1 / Pow(φ1, 2)) + 1 / v);

                // Calculate the new rating.
                double μʹ = μ + Pow(φʹ, 2) * s;

                rating.WorkingRating = μʹ * 173.7178 + StandardRating;
                rating.WorkingRatingDeviation = φʹ * 173.7178;
                rating.WorkingVolatility = σʹ;
            }
            else
            {
                // No results for this participant, so just update the RD.
                var φʹ = Sqrt(Pow(φ, 2) + Pow(σ, 2));

                rating.WorkingRatingDeviation = φʹ * 173.7178;
            }

            return rating;
        }

        /// <summary>
        /// Calculates the new volatility (σ′).
        /// </summary>
        protected double DetermineVolatility(double φ, double σ, double Δ, double v)
        {
            // Setting all of the variables.
            double Δsq = Pow(Δ, 2);
            double φsq = Pow(φ, 2);
            double τ = VolatilityConstraint;
            double ε = ConvergenceTolerance;
            // Let A = a = ln(σ^2)
            double A = Log(Pow(σ, 2));

            // and define
            double F(double x)
            {
                double n = Exp(x) * (Δsq - φsq - v - Exp(x));
                double d = 2 * Pow(φsq + v + Exp(x), 2);
                return n / d - (x - A) / Pow(τ, 2);
            }

            // Set the inital values of the iterative algorithm
            double B;

            if (Δsq > φsq + v)
            {
                B = Log(Δsq - φsq - v);
            }
            else
            {
                int k = 1;
                while (F(A - k * τ) < 0)
                {
                    k += 1;
                }

                B = A - k * τ;
            }

            // Let fA = f(A) and fB = f(B).
            double fA = F(A);
            double fB = F(B);

            double C;
            double fC;

            // While |B−A| > ε, carry out the following steps.
            while (Abs(B - A) > ε)
            {
                C = A + (A - B) * fA / (fB - fA);
                fC = F(C);

                if (fC * fB < 0)
                {
                    A = B;
                    fA = fB;
                }
                else
                {
                    fA /= 2;
                }

                B = C;
                fB = fC;
            }

            // The new volatility.
            return Exp(A / 2);
        }

        /// <summary>
        /// Reduces the impact of a result based on the opponent's φ (phi).
        /// </summary>
        protected double G(double φ)
        {
            return 1 / Sqrt(1 + 3 * Pow(φ, 2) / Pow(PI, 2));
        }

        /// <summary>
        /// Expects the score of a match based on each participant's μ (mu) and
        /// the reduced impact of the opponent's φ (phi) from <see cref='G'/>.
        /// </summary>
        protected double E(double μ, double μⱼ, double φⱼ)
        {
            return 1 / (1 + Exp(-G(φⱼ) * (μ - μⱼ)));
        }

        /// <summary>
        /// Calculates the Glicko-2 score from each participant's μ (mu), the
        /// opponent's φ (phi), and an actual score (e.g. 1 for win, 0 for loss,
        /// 0.5 for draw).
        /// </summary>
        protected double S(double μ, double μⱼ, double φⱼ, double score)
        {
            return G(φⱼ) * (score - E(μ, μⱼ, φⱼ));
        }

        /// <summary>
        /// Calculates the variance in rating from each participant's μ (mu) and 
        /// the opponent's φ (phi).
        /// </summary>
        protected double V(double μ, double μⱼ, double φⱼ)
        {
            return Pow(G(φⱼ), 2) * E(μ, μⱼ, φⱼ) * (1 - E(μ, μⱼ, φⱼ));
        }
    }
}
