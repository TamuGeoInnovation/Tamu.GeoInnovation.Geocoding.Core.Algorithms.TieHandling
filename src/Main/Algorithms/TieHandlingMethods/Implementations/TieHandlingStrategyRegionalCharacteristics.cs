using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Common.Core.Geocoders.ReferenceDatasets.Sources.Interfaces;
using USC.GISResearchLab.Common.Core.Geographics.Features.Streets;
using USC.GISResearchLab.Common.GeographicFeatures.Streets;
using USC.GISResearchLab.Common.Geometries;
using USC.GISResearchLab.Common.Geometries.Bearings;
using USC.GISResearchLab.Common.Geometries.Directions;
using USC.GISResearchLab.Common.Utils.Strings;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchingMethods;
using USC.GISResearchLab.Geocoding.Core.Metadata.FeatureMatchingResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.TieHandlingMethods.Implementations
{
    public class TieHandlingStrategyRegionalCharacteristics : AbstractTieHandlingStrategy
    {
        public TieHandlingStrategyRegionalCharacteristics()
        {
            TieHandlingStrategyType = TieHandlingStrategyType.RegionalCharacteristics;
        }

        public override FeatureMatchingResult HandleTie(ParameterSet parameterSet, IFeatureSource source, ReferenceSourceQueryResultSet candidates)
        {
            FeatureMatchingResult ret = new FeatureMatchingResult();

            try
            {
                List<MatchedFeature> matchedFeatures = candidates.GetTopCandidateTies();
                ret.FeatureMatchingResultCount = base.GetAmbiguityCount(candidates);
                ret.AmbiguityTypes = base.GetAmbiguityTypes(parameterSet.StreetAddress, matchedFeatures);
                ret.AmbiguousAddressComponents = base.GetAmbiguousAddressComponents(parameterSet.StreetAddress, matchedFeatures);
                ret.FeatureMatchingNotes = base.GetAmbiguityTypesAsString(ret.AmbiguityTypes);


                if (ret.AmbiguousAddressComponents.Count == 1)
                {
                    if (ret.AmbiguousAddressComponents.Contains(AddressComponents.Number)) // if the number is the problem, check the relation between the ranges to see if they tell us anything
                    {
                        ret = BreakTieNumber(parameterSet, source, matchedFeatures);
                    }

                    else if (ret.AmbiguousAddressComponents.Contains(AddressComponents.PreDirectional) || ret.AmbiguousAddressComponents.Contains(AddressComponents.PostDirectional)) // if the direcatinoal is the problem, check the relation between the features to see if they tell us anything
                    {
                        ret = BreakTieDirecational(parameterSet, source, matchedFeatures);
                    }
                }
                else if (ret.AmbiguousAddressComponents.Count == 2) // if there are two one needs to the number and the other should be a directional
                {
                    if (ret.AmbiguousAddressComponents.Contains(AddressComponents.Number))
                    {
                        if (ret.AmbiguousAddressComponents.Contains(AddressComponents.PreDirectional) || ret.AmbiguousAddressComponents.Contains(AddressComponents.PostDirectional))
                        {
                            ret = BreakTieDirecational(parameterSet, source, matchedFeatures);
                        }
                    }
                }

                ret.TieHandlingStrategyType = TieHandlingStrategyType;
                ret.FeatureMatchingGeographyType = source.FeatureMatchingGeographyType;
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

        public FeatureMatchingResult BreakTieNumber(ParameterSet parameterSet, IFeatureSource source, List<MatchedFeature> matchedFeatures)
        {
            FeatureMatchingResult ret = new FeatureMatchingResult();

            try
            {

                if (matchedFeatures.Count == 2)
                {
                    if (matchedFeatures[0].MatchedReferenceFeature.StreetAddressableGeographicFeature.GetType() == typeof(NickleStreet) && matchedFeatures[1].MatchedReferenceFeature.StreetAddressableGeographicFeature.GetType() == typeof(NickleStreet))
                    {

                        NickleStreet nickleStreetAddress1 = (NickleStreet)matchedFeatures[0].MatchedReferenceFeature.StreetAddressableGeographicFeature;
                        NickleStreet nickleStreetAddress2 = (NickleStreet)matchedFeatures[1].MatchedReferenceFeature.StreetAddressableGeographicFeature;

                        AddressRange addressRange1Major = nickleStreetAddress1.AddressRangeMajor;
                        AddressRange addressRange1MajorHouseNumber = nickleStreetAddress1.AddressRangeHouseNumberRangeMajor;
                        AddressRange addressRange1Minor = nickleStreetAddress1.AddressRangeMinor;
                        AddressRange addressRange1MinorHouseNumber = nickleStreetAddress1.AddressRangeHouseNumberRangeMinor;
                        AddressRange addressRange2Major = nickleStreetAddress2.AddressRangeMajor;
                        AddressRange addressRange2MajorHouseNumber = nickleStreetAddress2.AddressRangeHouseNumberRangeMajor;
                        AddressRange addressRange2Minor = nickleStreetAddress2.AddressRangeMinor;
                        AddressRange addressRange2MinorHouseNumber = nickleStreetAddress2.AddressRangeHouseNumberRangeMinor;

                        List<AddressRangeRelationship> relationships = new List<AddressRangeRelationship>();

                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1Major, addressRange2Major));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1Major, addressRange2Minor));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1Major, addressRange2MajorHouseNumber));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1Major, addressRange2MinorHouseNumber));

                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1MajorHouseNumber, addressRange2Major));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1MajorHouseNumber, addressRange2Minor));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1MajorHouseNumber, addressRange2MajorHouseNumber));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1MajorHouseNumber, addressRange2MinorHouseNumber));

                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1Minor, addressRange2Major));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1Minor, addressRange2Minor));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1Minor, addressRange2MajorHouseNumber));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1Minor, addressRange2MinorHouseNumber));

                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1MinorHouseNumber, addressRange2Major));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1MinorHouseNumber, addressRange2Minor));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1MinorHouseNumber, addressRange2MajorHouseNumber));
                        relationships.Add(AddressRangeComparator.GetRelationship(addressRange1MinorHouseNumber, addressRange2MinorHouseNumber));


                        // on equal, equalReversed, and disjoint, // nothing that can be done, punt
                        if (relationships.Contains(AddressRangeRelationship.Equal) || relationships.Contains(AddressRangeRelationship.EqualReversed))
                        {

                            if (relationships.Contains(AddressRangeRelationship.Equal))
                            {
                                ret.FeatureMatchingTieNotes = AddressRangeRelationship.Equal.ToString();
                            }
                            else if (relationships.Contains(AddressRangeRelationship.EqualReversed))
                            {
                                ret.FeatureMatchingTieNotes = AddressRangeRelationship.EqualReversed.ToString();
                            }

                            ret.MatchedFeatures.Add(new MatchedFeature());
                            ret.MatchedFeatures[0].MatchScore = matchedFeatures[0].MatchScore;
                            ret.FeatureMatchingResultType = FeatureMatchingResultType.Ambiguous;
                        }
                        else
                        {

                            if (relationships.Contains(AddressRangeRelationship.Contains))
                            {
                                ret.FeatureMatchingTieNotes = AddressRangeRelationship.Contains.ToString();
                                ret.MatchedFeatures.Add(matchedFeatures[1]); // use the one which is contained
                            }
                            else if (relationships.Contains(AddressRangeRelationship.ContainedBy))
                            {
                                ret.FeatureMatchingTieNotes = AddressRangeRelationship.ContainedBy.ToString();
                                ret.MatchedFeatures.Add(matchedFeatures[0]); // use the one which is contained
                            }
                            else if (relationships.Contains(AddressRangeRelationship.Intersects))
                            {
                                ret.FeatureMatchingTieNotes = AddressRangeRelationship.Intersects.ToString();
                            }
                            else if (relationships.Contains(AddressRangeRelationship.NextTo) || relationships.Contains(AddressRangeRelationship.Overlaps) || relationships.Contains(AddressRangeRelationship.DisjointFrom)) // next to, overlaps, and disjoint are handled the same
                            {
                                if (relationships.Contains(AddressRangeRelationship.Overlaps))
                                {
                                    ret.FeatureMatchingTieNotes = AddressRangeRelationship.Overlaps.ToString();
                                }
                                else if (relationships.Contains(AddressRangeRelationship.NextTo))
                                {
                                    ret.FeatureMatchingTieNotes = AddressRangeRelationship.NextTo.ToString();
                                }
                                else if (relationships.Contains(AddressRangeRelationship.DisjointFrom))
                                {
                                    ret.FeatureMatchingTieNotes = AddressRangeRelationship.DisjointFrom.ToString();
                                }

                                if (StringUtils.IsInt(parameterSet.StreetAddress.Number))
                                {
                                    int inputAddressNumber = Convert.ToInt32(parameterSet.StreetAddress.Number);

                                    int occurance = 0;
                                    if (relationships.Contains(AddressRangeRelationship.Overlaps))
                                    {
                                        occurance = relationships.LastIndexOf(AddressRangeRelationship.Overlaps);
                                    }
                                    else if (relationships.Contains(AddressRangeRelationship.NextTo))
                                    {
                                        occurance = relationships.LastIndexOf(AddressRangeRelationship.NextTo);
                                    }
                                    else if (relationships.Contains(AddressRangeRelationship.DisjointFrom))
                                    {
                                        occurance = relationships.LastIndexOf(AddressRangeRelationship.DisjointFrom);
                                    }

                                    AddressRange ar1 = null;
                                    AddressRange ar2 = null;

                                    switch (occurance)
                                    {
                                        case 0:
                                            ar1 = addressRange1Major;
                                            ar2 = addressRange2Major;
                                            break;
                                        case 1:
                                            ar1 = addressRange1Major;
                                            ar2 = addressRange2Minor;
                                            break;
                                        case 2:
                                            ar1 = addressRange1Major;
                                            ar2 = addressRange2MajorHouseNumber;
                                            break;
                                        case 3:
                                            ar1 = addressRange1Major;
                                            ar2 = addressRange2MinorHouseNumber;
                                            break;
                                        case 4:
                                            ar1 = addressRange1MajorHouseNumber;
                                            ar2 = addressRange2Major;
                                            break;
                                        case 5:
                                            ar1 = addressRange1MajorHouseNumber;
                                            ar2 = addressRange2Minor;
                                            break;
                                        case 6:
                                            ar1 = addressRange1MajorHouseNumber;
                                            ar2 = addressRange2MajorHouseNumber;
                                            break;
                                        case 7:
                                            ar1 = addressRange1MajorHouseNumber;
                                            ar2 = addressRange2MinorHouseNumber;
                                            break;
                                        case 8:
                                            ar1 = addressRange1Minor;
                                            ar2 = addressRange2Major;
                                            break;
                                        case 9:
                                            ar1 = addressRange1Minor;
                                            ar2 = addressRange2Minor;
                                            break;
                                        case 10:
                                            ar1 = addressRange1Minor;
                                            ar2 = addressRange2MajorHouseNumber;
                                            break;
                                        case 11:
                                            ar1 = addressRange1Minor;
                                            ar2 = addressRange2MinorHouseNumber;
                                            break;
                                        case 12:
                                            ar1 = addressRange1MinorHouseNumber;
                                            ar2 = addressRange2Major;
                                            break;
                                        case 13:
                                            ar1 = addressRange1MinorHouseNumber;
                                            ar2 = addressRange2Minor;
                                            break;
                                        case 14:
                                            ar1 = addressRange1MinorHouseNumber;
                                            ar2 = addressRange2MajorHouseNumber;
                                            break;
                                        case 15:
                                            ar1 = addressRange1MinorHouseNumber;
                                            ar2 = addressRange2MinorHouseNumber;
                                            break;
                                        default:
                                            throw new Exception("Could not find index of nextTo");
                                            break;
                                    }

                                    if (ar1 != null && ar2 != null)
                                    {
                                        int blockModulo = 1;
                                        // figure out what block number it's on (e.g., 2652 = 2600 block)
                                        if (inputAddressNumber > 100000)
                                        {
                                            blockModulo = 10000;
                                        }
                                        else if (inputAddressNumber > 10000)
                                        {
                                            blockModulo = 1000;
                                        }
                                        else if (inputAddressNumber > 1000)
                                        {
                                            blockModulo = 100;
                                        }
                                        else if (inputAddressNumber > 100)
                                        {
                                            blockModulo = 10;
                                        }
                                        else if (inputAddressNumber > 10)
                                        {
                                            blockModulo = 1;
                                        }
                                        else if (inputAddressNumber > 1)
                                        {
                                            blockModulo = 1;
                                        }

                                        int addressMod = inputAddressNumber / blockModulo;

                                        int ar1FromMod = ar1.FromAddress / blockModulo;
                                        int ar1ToMod = ar1.ToAddress / blockModulo;
                                        int ar2FromMod = ar2.FromAddress / blockModulo;
                                        int ar2ToMod = ar2.ToAddress / blockModulo;

                                        // assign it to the address range that has more of the correct block
                                        int ar1Count = 0;
                                        int ar2Count = 0;

                                        if (ar1FromMod == addressMod)
                                        {
                                            ar1Count++;
                                        }

                                        if (ar1ToMod == addressMod)
                                        {
                                            ar1Count++;
                                        }

                                        if (ar2FromMod == addressMod)
                                        {
                                            ar2Count++;
                                        }

                                        if (ar2ToMod == addressMod)
                                        {
                                            ar2Count++;
                                        }


                                        if (ar1Count != ar2Count)
                                        {
                                            if (ar1Count > ar2Count)
                                            {
                                                ret.MatchedFeatures.Add(matchedFeatures[0]);
                                            }
                                            else
                                            {
                                                ret.MatchedFeatures.Add(matchedFeatures[1]);
                                            }
                                            ret.FeatureMatchingResultType = FeatureMatchingResultType.BrokenTie;
                                        }
                                        else
                                        {
                                            // if both blocks are on the correct range (2650 input and 2600 block for both features), choose the smaller one if it is way smaller
                                            if (ar1.Size != ar2.Size)
                                            {
                                                if (ar1.Size < ar2.Size)
                                                {
                                                    double sizeRatio = Convert.ToDouble(ar2.Size) / Convert.ToDouble(ar1.Size);
                                                    if (sizeRatio > 2)
                                                    {
                                                        ret.MatchedFeatures.Add(matchedFeatures[0]);
                                                    }
                                                    else
                                                    {
                                                        ret.FeatureMatchingTieNotes += " - Niether blockrange is more similar";
                                                        ret.MatchedFeatures.Add(new MatchedFeature());
                                                        ret.MatchedFeatures[0].MatchScore = matchedFeatures[0].MatchScore;
                                                        ret.FeatureMatchingResultType = FeatureMatchingResultType.Ambiguous;
                                                    }
                                                }
                                                else
                                                {
                                                    double sizeRatio = Convert.ToDouble(ar1.Size) / Convert.ToDouble(ar2.Size);
                                                    if (sizeRatio > 2)
                                                    {
                                                        ret.MatchedFeatures.Add(matchedFeatures[1]);
                                                    }
                                                    else
                                                    {
                                                        ret.FeatureMatchingTieNotes += " - Niether blockrange is more similar";
                                                        ret.MatchedFeatures.Add(new MatchedFeature());
                                                        ret.MatchedFeatures[0].MatchScore = matchedFeatures[0].MatchScore;
                                                        ret.FeatureMatchingResultType = FeatureMatchingResultType.Ambiguous;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ret.FeatureMatchingTieNotes += " - Niether blockrange is more similar";
                                                ret.MatchedFeatures.Add(new MatchedFeature());
                                                ret.MatchedFeatures[0].MatchScore = matchedFeatures[0].MatchScore;
                                                ret.FeatureMatchingResultType = FeatureMatchingResultType.Ambiguous;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("one of the address ranges is null");

                                    }
                                }

                            }

                            ret.FeatureMatchingResultType = FeatureMatchingResultType.BrokenTie;
                        }
                    }
                }

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

        public FeatureMatchingResult BreakTieDirecational(ParameterSet parameterSet, IFeatureSource source, List<MatchedFeature> matchedFeatures)
        {
            FeatureMatchingResult ret = new FeatureMatchingResult();

            try
            {

                if (matchedFeatures.Count == 2)
                {

                    if (matchedFeatures[0].MatchedReferenceFeature.StreetAddressableGeographicFeature.GetType() == typeof(NickleStreet) && matchedFeatures[1].MatchedReferenceFeature.StreetAddressableGeographicFeature.GetType() == typeof(NickleStreet))
                    {

                        SqlGeography feature1 = matchedFeatures[0].MatchedReferenceFeature.StreetAddressableGeographicFeature.Geometry.SqlGeography;
                        SqlGeography feature2 = matchedFeatures[1].MatchedReferenceFeature.StreetAddressableGeographicFeature.Geometry.SqlGeography;

                        SqlGeography closestPoint1 = Geometry.GetPointOnAClosestsToB(feature1, feature2); // first try to use the points closest to each other
                        SqlGeography closestPoint2 = Geometry.GetPointOnAClosestsToB(feature2, feature1);

                        if (closestPoint1.Lat.Value == closestPoint2.Lat.Value && closestPoint1.Long.Value == closestPoint2.Long.Value) // if they intersect, we have to take the centroid
                        {
                            closestPoint1 = feature1.EnvelopeCenter();
                            closestPoint2 = feature2.EnvelopeCenter();
                        }

                        if (closestPoint1.Lat.Value != closestPoint2.Lat.Value || closestPoint1.Long.Value != closestPoint2.Long.Value) // if they still do not intersect
                        {

                            SqlGeography lineBetween = Geometry.BuildSqlGeographyLine(new SqlGeography[] { closestPoint1, closestPoint2 }, closestPoint1.STSrid.Value);

                            SqlGeography midPoint = Geometry.MidPointOfLine(lineBetween);

                            double bearing1 = Geometry.CalculateBearing(midPoint, closestPoint1);
                            double bearing2 = Geometry.CalculateBearing(midPoint, closestPoint2);

                            CardinalDirections fromMidTo1 = Bearing.GetDirectionFromBearing(bearing1);
                            CardinalDirections fromMidTo2 = Bearing.GetDirectionFromBearing(bearing2);

                            List<CardinalDirections> fromMidTo1List = CardinalDirection.GetDirectionComponents(fromMidTo1);
                            List<CardinalDirections> fromMidTo2List = CardinalDirection.GetDirectionComponents(fromMidTo2);

                            List<CardinalDirections> inputList = new List<CardinalDirections>();

                            if (!String.IsNullOrEmpty(parameterSet.StreetAddress.PreDirectional))
                            {
                                inputList.Add(CardinalDirection.GetDirectionFromName(parameterSet.StreetAddress.PreDirectional));
                            }

                            if (!String.IsNullOrEmpty(parameterSet.StreetAddress.PostDirectional))
                            {
                                inputList.Add(CardinalDirection.GetDirectionFromName(parameterSet.StreetAddress.PostDirectional));
                            }

                            int score1 = 0;
                            int score2 = 0;

                            foreach(CardinalDirections dir in inputList)
                            {
                                if (fromMidTo1List.Contains(dir))
                                {
                                    score1++;
                                }

                                if (fromMidTo2List.Contains(dir))
                                {
                                    score2++;
                                }
                            }

                            if (score1 != score2)
                            {
                                if (score1 > score2)
                                {
                                    ret.MatchedFeatures.Add(matchedFeatures[0]); 
                                }
                                else
                                {
                                    ret.MatchedFeatures.Add(matchedFeatures[1]); 
                                }
                                ret.FeatureMatchingResultType = FeatureMatchingResultType.BrokenTie;
                            }
                            else
                            {
                                ret.FeatureMatchingTieNotes = "Areas have the same directional character";
                                ret.MatchedFeatures.Add(new MatchedFeature());
                                ret.MatchedFeatures[0].MatchScore = matchedFeatures[0].MatchScore;
                                ret.FeatureMatchingResultType = FeatureMatchingResultType.Ambiguous;
                            }
                        }
                        else
                        {

                            ret.FeatureMatchingTieNotes = "Features intersect";
                            ret.MatchedFeatures.Add( new MatchedFeature());
                            ret.MatchedFeatures[0].MatchScore = matchedFeatures[0].MatchScore;
                            ret.FeatureMatchingResultType = FeatureMatchingResultType.Ambiguous;
                        }

                    }
                }

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
