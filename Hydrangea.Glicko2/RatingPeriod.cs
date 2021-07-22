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
using System.Collections.Immutable;
using System.Linq;

namespace Hydrangea.Glicko2
{
    /// <summary>
    /// Holds information about the current rating period.
    /// </summary>
    /// <remarks>
    /// This class should be instantiated through <see cref='Glicko2'/>,
    /// optionally using an object initializer to create the RatingPeriod
    /// with a premade <see cref='HashSet{T}'/> with <see cref='Result'/>
    /// instances, for example.
    /// </remarks>
    public class RatingPeriod
    {
        /// <summary>
        /// All of the <see cref='Result'/> objects recorded in this rating
        /// period, used to calculate the new <see cref='RatingInfo'/>
        /// of a participant depending on the score assigned to a 
        /// <see cref='RatingInfo'/> in <see cref='Result.Scores'/> after the
        /// rating period has ended.
        /// </summary>
        /// <value> 
        /// A <see cref='List{T}'/> containing <see cref='Result'/>
        /// objects.
        /// </value>
        public List<Result> Results { get; } = new();
        /// <summary>
        /// A convenience property for quickly getting all of the unique
        /// <see cref='RatingInfo'/> objects in <see cref='Results'/>.
        /// </summary>
        /// <value>
        /// A <see cref='HashSet{T}'/> containing <see cref='RatingInfo'/>
        /// objects.
        /// </value>
        public HashSet<RatingInfo> Participants { get; } = new();

        /// <summary>
        /// The default constructor, which leaves <see cref='Results'/> and
        /// <see cref='Participants'/> empty.
        /// </summary>
        public RatingPeriod() { }
        /// <summary>
        /// A constructor that uses <paramref name='results'/> to
        /// set the <see cref='Results'/> property. It also adds the
        /// <see cref='RatingInfo'/> instances in a <see cref='Result'/> to
        /// <see cref='Participants'/>, avoiding duplicates.
        /// </summary>
        /// <param name="results">
        /// A collection that implements <see cref='IList{T}'/> containing
        /// <see cref='Result'/> objects.
        /// </param>
        public RatingPeriod(IList<Result> results)
        {
            Results = results.ToList();
            foreach (var result in Results)
            {
                Participants.UnionWith(result.Participants);
            }
        }
        /// <summary>
        /// A constructor that uses <paramref name='results'/> and
        /// <paramref name='participants'/> to set the <see cref='Results'/>
        /// and <see cref='Participants'/> properties. 
        /// </summary>
        /// <param name="results">
        /// A collection that implements <see cref='IList{T}'/> containing
        /// <see cref='Result'/> objects.
        /// </param>
        /// <param name="participants">
        /// A collection that implements <see cref='ISet{T}'/> containing
        /// <see cref='RatingInfo'/> objects.
        /// </param>
        public RatingPeriod(IList<Result> results,
                            ISet<RatingInfo> participants)
        {
            Results = results.ToList();
            Participants = participants.ToHashSet();
        }

        /// <summary>
        /// Adds a <see cref='Result'/> to <see cref='Results'/> and
        /// the contents of <see cref='Result.Participants'/> to
        /// <see cref='Participants'/>, avoiding duplicates.
        /// </summary>
        /// <param name="result">
        /// A <see cref='Result'/> to add to <see cref='Results'/>.
        /// </param>
        public void AddResult(Result result)
        {
            Results.Add(result);
            AddParticipants(result.Participants);
        }
        /// <summary>
        /// Adds multiple <see cref='Result'/> objects to 
        /// <see cref='Results'/> and each <see cref='Result.Participants'/>
        /// to <see cref='Participants'/>, avoiding duplicates.
        /// </summary>
        /// <param name="results">
        /// A collection that implements <see cref='IList{T}'/> containing
        /// <see cref='Result'/> objects.
        /// </param>
        public void AddResults(IList<Result> results)
        {
            Results.AddRange(results);
            foreach (var result in Results)
            {
                AddParticipants(result.Participants);
            }
        }

        /// <summary>
        /// Adds a <see cref='RatingInfo'/> to <see cref='Participants'/>.
        /// </summary>
        /// <param name="participant">
        /// A <see cref='RatingInfo'/> to add to <see cref='Participants'/>.
        /// </param>
        public void AddParticipant(RatingInfo participant)
        {
            Participants.Add(participant);
        }
        /// <summary>
        /// Adds multiple <see cref='RatingInfo'/> objects to
        /// <see cref='Participants'/>, avoiding duplicates.
        /// </summary>
        /// <param name="participants">
        /// A collection that implements <see cref='ISet{T}'/> containing
        /// <see cref='RatingInfo'/> objects.
        /// </param>
        public void AddParticipants(ISet<RatingInfo> participants)
        {
            Participants.UnionWith(participants);
        }

        /// <summary>
        /// Returns every <see cref='Result'/> that has
        /// <paramref name='rating'/> in <see cref='Result.Participants'/>
        /// from <see cref='Results'/>.
        /// </summary>
        /// <param name="rating">
        /// A <see cref='RatingInfo'/> to get every result of.
        /// </param>
        /// <returns>
        /// A <see cref='List{T}'/> containing every <see cref='Result'/>
        /// the <paramref name='rating'/> was found in.
        /// </returns>
        public List<Result> GetParticipantResults(RatingInfo rating)
        {
            List<Result> resultList = new();

            foreach (var result in Results)
            {
                if (result.Participants.Contains(rating))
                {
                    resultList.Add(result);
                }
            }

            return resultList;
        }

        /// <summary>
        /// Clears both <see cref='Results'/> and <see cref='Participants'/>
        /// at the same time.
        /// </summary>
        public void Clear()
        {
            Results.Clear();
            Participants.Clear();
        }
    }

    /// <summary>
    /// Holds information about a match that was played.
    /// </summary>
    /// <example>
    /// The <see cref='Glicko2'/> class contains the appropriate scores for
    /// the results, so you should modify the class something like this:
    /// <code>
    /// var glicko2 = new Glicko2();
    /// var rating1 = glicko2.InitializeRating();
    /// var rating2 = glicko2.InitializeRating();
    /// var result = new Result();
    ///
    /// var finalResult = result.RecordScore(rating1, glicko2.WinScore)
    ///                         .RecordScore(rating2, glicko2.LossScore);
    /// glicko2.RecordResult(finalResult);
    /// </code>
    /// </example>
    public class Result
    {
        /// <summary>
        /// Used for storing the outcome of a match played between two
        /// participants to be used in calculating their new ratings.
        /// </summary>
        /// <value>
        /// An <see cref='ImmutableDictionary{TKey, TValue}'/> containing
        /// <see cref='RatingInfo'/> objects matched with a double.
        /// </value>
        public ImmutableDictionary<RatingInfo, double> Scores { get; }
        /// <summary>
        /// A convenience property for quickly getting all of the
        /// <see cref='RatingInfo'/> objects in <see cref='Scores'/>.
        /// </summary>
        /// <value>
        /// An <see cref='ImmutableHashSet{T}'/> containing
        /// <see cref='RatingInfo'/> objects.
        /// </value>
        public ImmutableHashSet<RatingInfo> Participants { get; }

        /// <summary>
        /// A constructor that uses two <see cref='RatingInfo'/> objects
        /// and two score values to set the <see cref='Scores'/> and
        /// <see cref='Participants'/> properties.
        /// </summary>
        /// <param name="participant1">
        /// A participant to be a key to the value of <paramref name='score1'/>.
        /// </param>
        /// <param name="participant2">
        /// A participant to be a key to the value of <paramref name='score2'/>.
        /// </param>
        /// <param name="score1">
        /// A score to be a value to the key of <paramref name='participant1'/>.
        /// </param>
        /// <param name="score2">
        /// A score to be a value to the key of <paramref name='participant2'/>.
        /// </param>
        public Result(RatingInfo participant1, 
                      RatingInfo participant2,
                      double score1,
                      double score2) 
        {
            Dictionary<RatingInfo, double> dict = new();
            dict.Add(participant1, score1);
            dict.Add(participant2, score2);
            Scores = dict.ToImmutableDictionary();
            Participants = Scores.Keys.ToImmutableHashSet();
        }

        /// <summary>
        /// Gets if the result was a draw by checking if all of the scores
        /// are the same. This usually means a draw.
        /// </summary>
        /// <returns>
        /// True if it was a draw.
        /// </returns>
        public bool IsDraw()
        {
            if (!Scores.Values.Distinct().Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
