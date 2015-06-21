using System;
using System.Collections.Generic;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchingMethods;
using USC.GISResearchLab.Geocoding.Core.Metadata.FeatureMatchingResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.TieHandlingMethods.Implementations
{
    public class TieHandlingStrategyReturnAll : AbstractTieHandlingStrategy
    {
        public TieHandlingStrategyReturnAll()
        {
            TieHandlingStrategyType = TieHandlingStrategyType.ReturnAll;
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

                ret.MatchedFeatures.AddRange(matchedFeatures);

                ret.TieHandlingStrategyType = TieHandlingStrategyType;
                ret.FeatureMatchingGeographyType = featureMatchingGeographyType;
                ret.FeatureMatchingResultType = FeatureMatchingResultType.Ambiguous;
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
