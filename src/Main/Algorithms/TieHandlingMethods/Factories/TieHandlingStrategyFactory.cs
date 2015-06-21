using System;
using USC.GISResearchLab.Geocoding.Core.Algorithms.TieHandlingMethods.Implementations;
using USC.GISResearchLab.Geocoding.Core.Algorithms.TieHandlingMethods.Interfaces;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.TieHandlingMethods.Factories
{
    public class TieHandlingStrategyFactory
    {
        public static ITieHandlingStrategy GetTieHandlingStrategy(TieHandlingStrategyType tieHandlingStrategyType)
        {
            ITieHandlingStrategy ret = null;

            switch (tieHandlingStrategyType)
            {
                case TieHandlingStrategyType.DynamicFeatureComposition:
                    ret = new TieHandlingStrategyDynamicFeatureComposition();
                    break;
                case TieHandlingStrategyType.FlipACoin:
                    ret = new TieHandlingStrategyFlipACoin();
                    break;
                    // DG Commented out becuase it created a circular reference with IFeatureSource. 2015-06-20
                //case TieHandlingStrategyType.RegionalCharacteristics:
                //    ret = new TieHandlingStrategyRegionalCharacteristics();
                //    break;
                case TieHandlingStrategyType.RevertToHierarchy:
                    ret = new TieHandlingStrategyRevertToHieararchy();
                    break;
                case TieHandlingStrategyType.ReturnAll:
                    ret = new TieHandlingStrategyReturnAll();
                    break;
                default:
                    throw new Exception("Unexpected or unimplemented TieHandlingStrategyType: " + tieHandlingStrategyType);
            }

            return ret;
        }
    }
}
