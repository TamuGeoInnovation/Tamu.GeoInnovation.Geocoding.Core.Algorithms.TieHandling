using System;
using System.Collections.Generic;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;

using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchingMethods;
using USC.GISResearchLab.Geocoding.Core.Metadata.FeatureMatchingResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.TieHandlingMethods.Implementations
{
    public class TieHandlingStrategyDynamicFeatureComposition : AbstractTieHandlingStrategy
    {

        public TieHandlingStrategyDynamicFeatureComposition()
        {
            TieHandlingStrategyType = TieHandlingStrategyType.DynamicFeatureComposition;
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


                CompositeMatchedFeature compositeFeature = new CompositeMatchedFeature();

                foreach (MatchedFeature matchedFeature in matchedFeatures)
                {
                    compositeFeature.AddMatchedFeature(matchedFeature);
                }

                compositeFeature.FeatureMatchTypes = FeatureMatchTypes.Composite;
                compositeFeature.MatchScore = candidates.GetTopCandidateScore();
                compositeFeature.BuildConvexHull();

                ret.TieHandlingStrategyType = TieHandlingStrategyType;
                ret.FeatureMatchingGeographyType = featureMatchingGeographyType;
                ret.FeatureMatchingResultType = FeatureMatchingResultType.Composite;
                ret.MatchedFeatures.Add(compositeFeature);
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
