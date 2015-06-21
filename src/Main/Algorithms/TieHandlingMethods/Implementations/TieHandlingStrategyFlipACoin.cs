using System;
using System.Collections.Generic;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchingMethods;
using USC.GISResearchLab.Geocoding.Core.Metadata.FeatureMatchingResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.TieHandlingMethods.Implementations
{
    public class TieHandlingStrategyFlipACoin : AbstractTieHandlingStrategy
    {
        public TieHandlingStrategyFlipACoin()
        {
            TieHandlingStrategyType = TieHandlingStrategyType.FlipACoin;
        }

        public override FeatureMatchingResult HandleTie(ParameterSet parameterSet, FeatureMatchingGeographyType featureMatchingGeographyType, ReferenceSourceQueryResultSet candidates)
        {
            
            FeatureMatchingResult ret = new FeatureMatchingResult();

            try
            {
                List<MatchedFeature> matchedFeatures = candidates.GetTopCandidateTies();
                ret.FeatureMatchingResultCount = base.GetAmbiguityCount(candidates);
                ret.AmbiguityTypes = base.GetAmbiguityTypes(parameterSet.StreetAddress, matchedFeatures);
                ret.AmbiguousAddressComponents = base.GetAmbiguousAddressComponents(parameterSet.StreetAddress, matchedFeatures);
                ret.FeatureMatchingNotes = base.GetAmbiguityTypesAsString(ret.AmbiguityTypes);

                double count = Convert.ToDouble(ret.FeatureMatchingResultCount);
                double[] probabilities = new double[ret.FeatureMatchingResultCount];

                for (int i = 0; i < probabilities.Length; i++)
                {
                    probabilities[i] = Convert.ToDouble(i) * (1 / count);
                }

                Random random = new Random(DateTime.Now.Millisecond);
                double randomValue = random.NextDouble();

                int outputIndex = 0;
                for (int i = 0; i < probabilities.Length; i++)
                {
                    if (i != probabilities.Length - 1) // whebn on the last one, just return it
                    {
                        if (randomValue >= probabilities[i] && randomValue <= probabilities[i + 1]) // 0 - .5
                        {
                            outputIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        outputIndex = i;
                        break;
                    }
                }

                ret.MatchedFeatures.Add(matchedFeatures[outputIndex]);

                ret.TieHandlingStrategyType = TieHandlingStrategyType;
                ret.FeatureMatchingGeographyType = featureMatchingGeographyType;
                ret.FeatureMatchingResultType = FeatureMatchingResultType.BrokenTie;
            }
            catch (Exception e)
            {
                ret.FeatureMatchingResultType = FeatureMatchingResultType.ExceptionOccurred;
                ret.Error = "HandleTie() - Error handling tie: " + e.Message;
                ret.ExceptionOccurred = true;
                ret.Exception = e;
            }

            return ret;
        }
    }
}
