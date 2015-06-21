using System;
using System.Collections.Generic;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;

using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchingMethods;
using USC.GISResearchLab.Geocoding.Core.Algorithms.TieHandlingMethods.Interfaces;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;
using USC.GISResearchLab.Geocoding.Core.Metadata.FeatureMatchingResults;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.TieHandlingMethods
{

    public abstract class AbstractTieHandlingStrategy : ITieHandlingStrategy
    {


        #region Properties

        public TieHandlingStrategyType TieHandlingStrategyType { get; set; }

        #endregion

        public abstract FeatureMatchingResult HandleTie(ParameterSet parameterSet, FeatureMatchingGeographyType featureMatchingGeographyType, ReferenceSourceQueryResultSet candidates);

        public int GetAmbiguityCount(ReferenceSourceQueryResultSet candidates)
        {
            int ret = 0;

            if (candidates != null)
            {
                List<MatchedFeature> matchedFeatures = candidates.GetTopCandidateTies();
                if (matchedFeatures != null)
                {
                    ret = matchedFeatures.Count;
                }
            }

            return ret;
        }

        public List<AddressComponents> GetAmbiguousAddressComponents(StreetAddress inputAddress, List<MatchedFeature> matchedFeatures)
        {
            FeatureMatchingAmbiguousResult ambiguousResult = new FeatureMatchingAmbiguousResult();
            return ambiguousResult.DetermineAmbiguousAddressComponents(inputAddress, matchedFeatures);
        }

        public List<FeatureMatchingAmbiguity> GetAmbiguityTypes(StreetAddress inputAddress, List<MatchedFeature> matchedFeatures)
        {
            FeatureMatchingAmbiguousResult ambiguousResult = new FeatureMatchingAmbiguousResult();
            return ambiguousResult.DetermineAmbiguityType(inputAddress, matchedFeatures);
        }

        public string GetAmbiguityTypesAsString(List<FeatureMatchingAmbiguity> ambiguityTypes)
        {
            string ret  = "";
            if (ambiguityTypes != null)
            {
                foreach (FeatureMatchingAmbiguity ambiguity in ambiguityTypes)
                {
                    if (!String.IsNullOrEmpty(ret))
                    {
                        ret += ";";
                    }

                    ret += ambiguity.AddressComponent + "-" + ambiguity.FeatureMatchingAmbiguityType;
                }
            }
            return ret;
        }
    }
}
