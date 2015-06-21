using System;
using System.Collections.Generic;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchingMethods;
using USC.GISResearchLab.Geocoding.Core.Metadata.FeatureMatchingResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.TieHandlingMethods.Implementations
{
    public class TieHandlingStrategyRevertToHieararchy : AbstractTieHandlingStrategy
    {
        public TieHandlingStrategyRevertToHieararchy()
        {
            TieHandlingStrategyType = TieHandlingStrategyType.RevertToHierarchy;
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

                ret.TieHandlingStrategyType = TieHandlingStrategyType;

                ret.MatchedFeatures.Add(new MatchedFeature());
                ret.MatchedFeatures[0].MatchScore = candidates.GetTopCandidateScore();
                ret.FeatureMatchingResultType = FeatureMatchingResultType.Ambiguous;
                ret.Error = "Ambigous match for top candidate score";
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
