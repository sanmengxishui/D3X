using HalconDotNet;
using JPTCG.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPTCG.Vision
{
    public partial class HalconInspection
    {

        public class RectData
        {
            public bool Found;
            public double X;
            public double Y;
            public double Height;
            public double Width;
            public double Angle;
            public double Radius;
            public double Means;
            public bool isCircle;
            public HObject rect_border = new HObject();
            public HObject InspectedImage = new HObject();
            //public HObject Mod2InspectedImage = new HObject();
        }



        public class GreyValueData
        {
            public double min;
            public double max;
            public double mean;
        }

        public HalconInspection()
        {
            //RectData    
        }

        public GreyValueData MinMaxMeanGreyValue(HObject ho_Image)
        {
            GreyValueData myData = new GreyValueData();
            HTuple hv_Width, hv_Height, hv_Mean, hv_Deviation;
            HTuple hv_Min, hv_Max, hv_percentage, hv_range;

            hv_Width = new HTuple();
            hv_Height = new HTuple();
            hv_Mean = new HTuple();
            hv_Deviation = new HTuple();
            hv_Min = new HTuple();
            hv_Max = new HTuple();
            hv_percentage = new HTuple();
            hv_range = new HTuple();
            HOperatorSet.Intensity(ho_Image, ho_Image, out hv_Mean, out hv_Deviation);
            HOperatorSet.MinMaxGray(ho_Image, ho_Image, 0, out hv_Min, out hv_Max, out hv_range);

            myData.max = hv_Max.D;
            myData.min = hv_Min.D;
            myData.mean = hv_Mean.D;

            return myData;

        }
        public RectData FindRect(HObject ho_Image, int threshold)
        {
            RectData myResult = new RectData();
            myResult.Found = false;

            try
            {
                HTuple hv_Width, hv_Height, hv_Y, hv_X, hv_AngRad, hv_HalfWidth, hv_HalfLength, hv_PtOrder, hv_Mean, hv_Deviation;
                HObject hv_border, hv_selectedShape, hv_connection, ho_Rectangle, ho_ImageReduced;
                hv_Width = new HTuple();
                hv_Height = new HTuple();
                hv_Y = new HTuple();
                hv_X = new HTuple();
                hv_AngRad = new HTuple();
                hv_HalfWidth = new HTuple();
                hv_HalfLength = new HTuple();
                hv_PtOrder = new HTuple();
                HTuple hv_Number = new HTuple();
                hv_Mean = new HTuple(); hv_Deviation = new HTuple();
                HOperatorSet.GenEmptyObj(out hv_border);
                HOperatorSet.GenEmptyObj(out hv_selectedShape);
                HOperatorSet.GenEmptyObj(out hv_connection);
                HOperatorSet.GenEmptyObj(out ho_Rectangle);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced);

                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                HOperatorSet.SetWindowAttr("background_color", "black");

                HTuple hv_Row, hv_Column, hv_Phi;
                HTuple hv_Length1, hv_Length2;
                //Create template
                hv_Row = 972;
                hv_Column = 1290;
                hv_Phi = 0;
                hv_Length1 = 1270;
                hv_Length2 = 500;

                string DataFileName = @"D:\Images\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
                //if (!Directory.Exists(DataFileName))
                //    Directory.CreateDirectory(DataFileName);
                //string temp1 = DataFileName + "Inspected" + DateTime.Now.ToString("HH_mm_ss");
                //HOperatorSet.WriteImage(ho_Image, "bmp", 0, temp1);

                ho_Rectangle.Dispose();
                HOperatorSet.GenRectangle2(out ho_Rectangle, hv_Row, hv_Column, hv_Phi, hv_Length1, hv_Length2);

                //ho_ImageReduced.Dispose();
                //HOperatorSet.ReduceDomain(ho_Image, ho_Rectangle, out ho_ImageReduced);

                //string temp1 = DataFileName + "ReducedInspected" + DateTime.Now.ToString("HH_mm_ss");
                //HOperatorSet.WriteImage(ho_ImageReduced, "bmp", 0, temp1);

                HOperatorSet.ThresholdSubPix(ho_Image, out hv_border, threshold);
                //HOperatorSet.EdgesSubPix(ho_Image, out hv_border, "canny", 0.9, 20, 40);
                HOperatorSet.SelectShapeXld(hv_border, out hv_selectedShape, "area", "and", 250000, 600000); //Alvin 28July16 2500


                HOperatorSet.CountObj(hv_selectedShape, out hv_Number);

                HOperatorSet.FitRectangle2ContourXld(hv_selectedShape, "tukey", -1, 0, 0, 3, 2, out hv_Y, out hv_X, out hv_AngRad,
                                                    out hv_HalfWidth, out hv_HalfLength, out hv_PtOrder);

                if (hv_X.Length != 0)
                {
                    for (int i = 0; i < hv_X.Length; i++)
                    {
                        ho_Rectangle.Dispose();
                        HOperatorSet.GenRectangle2(out ho_Rectangle, hv_Y[i].D, hv_X[i].D, hv_AngRad[i].D, hv_HalfWidth[i].D, hv_HalfLength[i].D);
                        //HOperatorSet.GenRectangle2(out ho_Rectangle, hv_Y[i], hv_X[i], hv_AngRad[i], hv_HalfWidth[i], hv_HalfLength[i]);  
                        //string temp1 = DataFileName + "InspectedRectangle" + DateTime.Now.ToString("HH_mm_ss");
                        //HOperatorSet.WriteImage(ho_Rectangle, "bmp", 0, temp1);
                        HOperatorSet.Intensity(ho_Rectangle, ho_Image, out hv_Mean, out hv_Deviation);
                        myResult.Means = hv_Mean.D;

                        if (myResult.Means < 220)
                        {
                            myResult.Found = true;
                            myResult.X = hv_X[i].D;
                            myResult.Y = hv_Y[i].D;
                            myResult.Angle = hv_AngRad[i].D;
                            myResult.Width = hv_HalfWidth[i].D * 2;
                            myResult.Height = hv_HalfLength[i].D * 2;
                            myResult.isCircle = false;
                            //myResult.rect_border = ho_Rectangle;//hv_selectedShape;

                            if ((myResult.X < 200) || (myResult.Height < 170) || (myResult.Y < 300))
                                myResult.Found = false;
                            else
                                break;
                        }

                    }
                }


            }
            catch (Exception ex)
            { }

            return myResult;

            //}
            //catch (HalconException HDevExpDefaultException)
            //{
            //    return myResult;
            //}
        }

        public double GetPictureMeans(HObject ho_Image)
        {
            double res = 0;
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_Mean = new HTuple(), hv_Deviation = new HTuple();
            HObject ho_Rectangle;
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            HOperatorSet.GenRectangle2(out ho_Rectangle, (hv_Width / 2), (hv_Height / 2), 0, (hv_Width / 2), (hv_Height / 2));
            HOperatorSet.Intensity(ho_Rectangle, ho_Image, out hv_Mean, out hv_Deviation);
            return res = hv_Mean.D;
        }
        /// <summary>
        /// 白点检测20171010
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="ho_Image"></param>
        /// <param name="pixel_MM"></param>
        /// <returns></returns>
        public WhiteParaList WhiteDotInspect(string barcode, string modle, HObject ho_Image, double pixel_MM, string ImagePath)
        {

            WhiteParaList WhiteParaReturn = new WhiteParaList();
            // Local iconic variables 

            HObject ho_Domain, ho_ImageScaled = null, ho_SelectObjregion;
            HObject ho_ImageInvert, ho_Region2, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_RegionDilation = null, ho_Circle = null;
            HObject ho_ImageResult = null;

            // Local control variables 

            HTuple hv_Name = null, hv_Width = null, hv_Height = null, hv_RowY = null, hv_ColumnX = null, hv_Area1 = null;
            HTuple hv_Index1 = null, hv_Mean1 = new HTuple(), hv_Deviation1 = new HTuple();
            HTuple hv_radius_MM = null, hv_radius_Pixel = null, hv_area_Pixel = null;
            HTuple hv_Number = null, hv_NumDot = null, hv_Row = new HTuple();
            HTuple hv_Column = new HTuple(), hv_Radius = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_SelectObjregion);
            HOperatorSet.GenEmptyObj(out ho_Domain);
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_ImageInvert);
            HOperatorSet.GenEmptyObj(out ho_Region2);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.GenEmptyObj(out ho_ImageResult);
            hv_Name = barcode;
            hv_NumDot = 0;
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            ho_Domain.Dispose();
            HOperatorSet.GetDomain(ho_Image, out ho_Domain);
            for (hv_Index1 = 0; (int)hv_Index1 <= 255; hv_Index1 = (int)hv_Index1 + 1)
            {
                ho_ImageScaled.Dispose();
                HOperatorSet.ScaleImage(ho_Image, out ho_ImageScaled, 1.6, -hv_Index1);
                HOperatorSet.Intensity(ho_Domain, ho_ImageScaled, out hv_Mean1, out hv_Deviation1);
                if ((int)((new HTuple(hv_Mean1.TupleGreater(20))).TupleAnd(new HTuple(hv_Mean1.TupleLess(
                    30)))) != 0)
                {
                    break;
                }
            }
            ho_ImageInvert.Dispose();
            HOperatorSet.InvertImage(ho_ImageScaled, out ho_ImageInvert);
            ho_Region2.Dispose();
            HOperatorSet.VarThreshold(ho_ImageInvert, out ho_Region2, hv_Width, hv_Height,
                2, 100, "dark");
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Region2, out ho_ConnectedRegions);
            hv_radius_MM = pixel_MM * 1000;
            hv_radius_Pixel = 20 / hv_radius_MM;
            hv_area_Pixel = (3.14 * hv_radius_Pixel) * hv_radius_Pixel;
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area",
                "and", hv_area_Pixel, 99999);
            HOperatorSet.CountObj(ho_SelectedRegions, out hv_Number);
            hv_NumDot = hv_Number.Clone();
            if (hv_NumDot.I > 5)
            {
                WhiteParaReturn.whiteCounts =5;
            }
            else
            WhiteParaReturn.whiteCounts = hv_NumDot.I;
            if ((int)(new HTuple(hv_NumDot.TupleGreater(0))) != 0)
            {
                ho_RegionDilation.Dispose();
                HOperatorSet.DilationCircle(ho_SelectedRegions, out ho_RegionDilation, 5);
                HOperatorSet.SmallestCircle(ho_RegionDilation, out hv_Row, out hv_Column, out hv_Radius);
                ho_Circle.Dispose();
                HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                ho_ImageResult.Dispose();
                HOperatorSet.PaintRegion(ho_Circle, ho_Image, out ho_ImageResult, 0, "margin");


                string fileName = ImagePath + "\\" + barcode + ".bmp";

                HOperatorSet.WriteImage(ho_ImageResult, "bmp", 0, fileName);

                string fileName1 = ImagePath + "\\" + "Original" + barcode + ".bmp";

                HOperatorSet.WriteImage(ho_Image, "bmp", 0, fileName1);
                if (hv_NumDot.I > 5)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        ho_SelectObjregion.Dispose();
                        HOperatorSet.SelectObj(ho_SelectedRegions, out ho_SelectObjregion, j + 1);
                        HOperatorSet.AreaCenter(ho_SelectObjregion, out hv_Area1, out hv_RowY, out hv_ColumnX);
                        WhiteParaReturn.whiteX[j] = (float)Math.Round(hv_ColumnX.D * pixel_MM, 3);
                        WhiteParaReturn.whiteY[j] = (float)Math.Round(hv_RowY.D * pixel_MM, 3);
                        WhiteParaReturn.whiteArea[j] = (float)Math.Round(hv_Area1.D * pixel_MM * pixel_MM, 3);
                    }
                }
                else
                {
                    for (int j = 0; j < hv_NumDot.I; j++)
                    {
                        ho_SelectObjregion.Dispose();
                        HOperatorSet.SelectObj(ho_SelectedRegions, out ho_SelectObjregion, j + 1);
                        HOperatorSet.AreaCenter(ho_SelectObjregion, out hv_Area1, out hv_RowY, out hv_ColumnX);
                        WhiteParaReturn.whiteX[j] = (float)Math.Round(hv_ColumnX.D * pixel_MM, 3);
                        WhiteParaReturn.whiteY[j] = (float)Math.Round(hv_RowY.D * pixel_MM, 3);
                        WhiteParaReturn.whiteArea[j] = (float)Math.Round(hv_Area1.D * pixel_MM * pixel_MM, 3);
                    }
                }



            }
            //else
            //{

            //    string path = "G:\\BlackAndWhiteDotImage";
            //    if (!Directory.Exists(path))
            //    {
            //        Directory.CreateDirectory(path);
            //    }
            //    string s_FileName = path + "\\" + DateTime.Now.ToString("yyyyMMdd");
            //    if (!Directory.Exists(s_FileName))
            //    {
            //        Directory.CreateDirectory(s_FileName);
            //    }
            //    string FileName = s_FileName + "\\" + "WhiteDotImage";
            //    if (!Directory.Exists(FileName))
            //    {
            //        Directory.CreateDirectory(FileName);
            //    }
            //    string fileName = FileName + "\\" + barcode + ".bmp";

            //    HOperatorSet.WriteImage(ho_Image, "bmp", 0, fileName);



            //}
            ho_SelectObjregion.Dispose();
            ho_Domain.Dispose();
            ho_ImageScaled.Dispose();
            ho_ImageInvert.Dispose();
            ho_Region2.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_RegionDilation.Dispose();
            ho_Circle.Dispose();
            ho_ImageResult.Dispose();
            return WhiteParaReturn;
        }

        public WhiteParaList WhiteDotInspect1(string barcode, string modle, HObject ho_Image, double pixel_MM, string ImagePath)
        {

            WhiteParaList WhiteParaReturn = new WhiteParaList();
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];
            long SP_O = 0;

            // Local iconic variables 


            // Initialize local and output iconic variables 


            HObject ho_EmptyRegion, ho_Region, ho_SelectObjregion;
            HObject ho_RegionFillUp, ho_RegionErosion, ho_RegionDifference;
            HObject ho_ImageReduced, ho_Region1, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_ImageReduced1, ho_ImageEmphasize;
            HObject ho_Region2, ho_ConnectedRegions1, ho_SelectedRegions1;
            HObject ho_RegionDilation = null, ho_Circle = null, ho_ImageResult = null;

            HObject ho_Domain11, ho_ImageScaled;


            // Local control variables 

            HTuple hv_Width, hv_Height, hv_Mean, hv_Deviation, hv_RowY = null, hv_ColumnX = null, hv_Area1 = null;;
            HTuple hv_meanResult, hv_Number, hv_Number1, hv_Number2, hv_NumDot = null;
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple(), hv_Radius = new HTuple();
            HTuple hv_Index = new HTuple();

            HTuple hv_AbsoluteHisto, hv_RelativeHisto;
            HTuple hv_Max, hv_maxGrayValue;

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_SelectObjregion);
            HOperatorSet.GenEmptyObj(out ho_EmptyRegion);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            HOperatorSet.GenEmptyObj(out ho_ImageEmphasize);
            HOperatorSet.GenEmptyObj(out ho_Region2);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.GenEmptyObj(out ho_ImageResult);

        
            HOperatorSet.GenEmptyObj(out ho_Domain11);
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);

            HTuple hv_radius_MM = null, hv_area_Pixel = null;

            hv_radius_MM = pixel_MM;
            hv_area_Pixel = 0.005 / (pixel_MM * pixel_MM);

            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            ho_EmptyRegion.Dispose();
            HOperatorSet.GenEmptyRegion(out ho_EmptyRegion);


            ho_Domain11.Dispose();
            HOperatorSet.GetDomain(ho_Image, out ho_Domain11);
            HOperatorSet.GrayHisto(ho_Domain11, ho_Image, out hv_AbsoluteHisto, out hv_RelativeHisto);
            HOperatorSet.TupleMax(hv_AbsoluteHisto, out hv_Max);
            hv_maxGrayValue = hv_AbsoluteHisto.TupleFind(hv_Max);
            ho_ImageScaled.Dispose();
            HOperatorSet.ScaleImage(ho_Image, out ho_ImageScaled, 1, 140 - hv_maxGrayValue[0]);

            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_ImageScaled, out ho_Region, 80, 255);
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_Region, out ho_RegionFillUp);
            ho_RegionErosion.Dispose();
            HOperatorSet.ErosionCircle(ho_RegionFillUp, out ho_RegionErosion, 30);
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_RegionFillUp, ho_RegionErosion, out ho_RegionDifference
                );
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_Image, ho_RegionDifference, out ho_ImageReduced);
            HOperatorSet.Intensity(ho_RegionDifference, ho_ImageReduced, out hv_Mean, out hv_Deviation);
            hv_meanResult = hv_Mean * 1.6;
            if ((int)(new HTuple(hv_meanResult.TupleGreater(240))) != 0)
            {
                hv_meanResult = 240;
            }
            ho_Region1.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced, out ho_Region1, hv_meanResult, 255);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Region1, out ho_ConnectedRegions);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area",
                "and", hv_area_Pixel, 99999);
            HOperatorSet.CountObj(ho_SelectedRegions, out hv_Number);
            if ((int)(new HTuple(hv_Number.TupleGreater(0))) != 0)
            {
                OTemp[SP_O] = ho_EmptyRegion.CopyObj(1, -1);
                SP_O++;
                ho_EmptyRegion.Dispose();
                HOperatorSet.ConcatObj(OTemp[SP_O - 1], ho_SelectedRegions, out ho_EmptyRegion
                    );
                OTemp[SP_O - 1].Dispose();
                SP_O = 0;
            }
            ho_ImageReduced1.Dispose();
            HOperatorSet.ReduceDomain(ho_Image, ho_RegionErosion, out ho_ImageReduced1);
            ho_ImageEmphasize.Dispose();
            HOperatorSet.Emphasize(ho_ImageReduced1, out ho_ImageEmphasize, 11, 11, 1);
            ho_Region2.Dispose();
            HOperatorSet.VarThreshold(ho_ImageEmphasize, out ho_Region2, hv_Width, hv_Height,
                0.2, 120, "light");
            ho_ConnectedRegions1.Dispose();
            HOperatorSet.Connection(ho_Region2, out ho_ConnectedRegions1);
            ho_SelectedRegions1.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions1, "area",
                "and", hv_area_Pixel, 99999);
            HOperatorSet.CountObj(ho_SelectedRegions1, out hv_Number1);
            if ((int)(new HTuple(hv_Number1.TupleGreater(0))) != 0)
            {
                OTemp[SP_O] = ho_EmptyRegion.CopyObj(1, -1);
                SP_O++;
                ho_EmptyRegion.Dispose();
                HOperatorSet.ConcatObj(OTemp[SP_O - 1], ho_SelectedRegions1, out ho_EmptyRegion
                    );
                OTemp[SP_O - 1].Dispose();
                SP_O = 0;
            }
            HOperatorSet.CountObj(ho_EmptyRegion, out hv_Number2);
            hv_NumDot = hv_Number2-1;
            if (hv_NumDot.I > 5)
            {
                WhiteParaReturn.whiteCounts = 5;
            }
            else

            WhiteParaReturn.whiteCounts = hv_NumDot.I;
            if ((int)(new HTuple(hv_Number2.TupleGreater(1))) != 0)
            {


                ho_RegionDilation.Dispose();
                HOperatorSet.DilationCircle(ho_EmptyRegion, out ho_RegionDilation, 5);
                HOperatorSet.SmallestCircle(ho_RegionDilation, out hv_Row, out hv_Column, out hv_Radius);
                ho_Circle.Dispose();
                HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                ho_ImageResult.Dispose();
                HOperatorSet.PaintRegion(ho_Circle, ho_Image, out ho_ImageResult, 0, "margin");
              

                string fileName = ImagePath + "\\" + barcode + ".bmp";

                HOperatorSet.WriteImage(ho_ImageResult, "bmp", 0, fileName);

                string fileName1 = ImagePath + "\\" + "Original" + barcode + ".bmp";

                HOperatorSet.WriteImage(ho_Image, "bmp", 0, fileName1);

                if (hv_NumDot.I > 5)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        ho_SelectObjregion.Dispose();
                        HOperatorSet.SelectObj(ho_EmptyRegion ,out ho_SelectObjregion, j +2);
                        HOperatorSet.AreaCenter(ho_SelectObjregion, out hv_Area1, out hv_RowY, out hv_ColumnX);
                        WhiteParaReturn.whiteX[j] = (float)Math.Round(hv_ColumnX.D * pixel_MM, 3);
                        WhiteParaReturn.whiteY[j] = (float)Math.Round(hv_RowY.D * pixel_MM, 3);
                        WhiteParaReturn.whiteArea[j] = (float)Math.Round(hv_Area1.D * pixel_MM * pixel_MM, 3);
                    }
                }
                else
                {
                    for (int j = 0; j < hv_NumDot.I; j++)
                    {
                        ho_SelectObjregion.Dispose();
                        HOperatorSet.SelectObj(ho_EmptyRegion, out ho_SelectObjregion, j + 2);
                        HOperatorSet.AreaCenter(ho_SelectObjregion, out hv_Area1, out hv_RowY, out hv_ColumnX);
                        WhiteParaReturn.whiteX[j] = (float)Math.Round(hv_ColumnX.D * pixel_MM, 3);
                        WhiteParaReturn.whiteY[j] = (float)Math.Round(hv_RowY.D * pixel_MM, 3);
                        WhiteParaReturn.whiteArea[j] = (float)Math.Round(hv_Area1.D * pixel_MM * pixel_MM, 3);
                    }
                }
            }

            ho_SelectObjregion.Dispose();
            ho_EmptyRegion.Dispose();
            ho_Region.Dispose();
            ho_RegionFillUp.Dispose();
            ho_RegionErosion.Dispose();
            ho_RegionDifference.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region1.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_ImageReduced1.Dispose();
            ho_ImageEmphasize.Dispose();
            ho_Region2.Dispose();
            ho_ConnectedRegions1.Dispose();
            ho_SelectedRegions1.Dispose();
            ho_RegionDilation.Dispose();
            ho_Circle.Dispose();
            ho_ImageResult.Dispose();

            ho_Domain11.Dispose();
            ho_ImageScaled.Dispose();
            return WhiteParaReturn;
        }
        public WhiteParaList WhiteDotInspectA(string barcode, string modle, HObject ho_Image, double pixel_MM, string ImagePath)
        {

            WhiteParaList WhiteParaReturn = new WhiteParaList();
            try
            {
                // Stack for temporary objects 
                HObject[] OTemp = new HObject[20];
                long SP_O = 0;

                // Local iconic variables 


                // Initialize local and output iconic variables 


                HObject ho_EmptyRegion, ho_Region, ho_SelectObjregion;
                HObject ho_RegionFillUp, ho_RegionErosion, ho_RegionDifference;
                HObject ho_ImageReduced, ho_Region1, ho_ConnectedRegions;
                HObject ho_SelectedRegions, ho_ImageReduced1, ho_ImageEmphasize;
                HObject ho_Region2, ho_ConnectedRegions1, ho_SelectedRegions1;
                HObject ho_RegionDilation = null, ho_Circle = null, ho_ImageResult = null;

                HObject ho_Domain11, ho_ImageScaled;
                HObject ho_SortRegion;


                // Local control variables 

                HTuple hv_Width, hv_Height, hv_Mean, hv_Deviation, hv_RowY = null, hv_ColumnX = null, hv_Area1 = null; ;
                HTuple hv_meanResult, hv_Number, hv_Number1, hv_Number2, hv_NumDot = null;
                HTuple hv_Row = new HTuple(), hv_Column = new HTuple(), hv_Radius = new HTuple();
                HTuple hv_Index = new HTuple();

                HTuple hv_AbsoluteHisto, hv_RelativeHisto;
                HTuple hv_Max, hv_maxGrayValue;

                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_SelectObjregion);
                HOperatorSet.GenEmptyObj(out ho_EmptyRegion);
                HOperatorSet.GenEmptyObj(out ho_Region);
                HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
                HOperatorSet.GenEmptyObj(out ho_RegionErosion);
                HOperatorSet.GenEmptyObj(out ho_RegionDifference);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced);
                HOperatorSet.GenEmptyObj(out ho_Region1);
                HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
                HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
                HOperatorSet.GenEmptyObj(out ho_ImageEmphasize);
                HOperatorSet.GenEmptyObj(out ho_Region2);
                HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
                HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
                HOperatorSet.GenEmptyObj(out ho_RegionDilation);
                HOperatorSet.GenEmptyObj(out ho_Circle);
                HOperatorSet.GenEmptyObj(out ho_ImageResult);


                HOperatorSet.GenEmptyObj(out ho_Domain11);
                HOperatorSet.GenEmptyObj(out ho_ImageScaled);
                HOperatorSet.GenEmptyObj(out ho_SortRegion);

                HTuple hv_radius_MM = null, hv_area_Pixel = null;

                hv_radius_MM = pixel_MM;
                hv_area_Pixel = 0.005 / (pixel_MM * pixel_MM);

                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                ho_EmptyRegion.Dispose();
                HOperatorSet.GenEmptyRegion(out ho_EmptyRegion);


                ho_Domain11.Dispose();
                HOperatorSet.GetDomain(ho_Image, out ho_Domain11);
                HOperatorSet.GrayHisto(ho_Domain11, ho_Image, out hv_AbsoluteHisto, out hv_RelativeHisto);
                HOperatorSet.TupleMax(hv_AbsoluteHisto, out hv_Max);
                hv_maxGrayValue = hv_AbsoluteHisto.TupleFind(hv_Max);
                ho_ImageScaled.Dispose();
                HOperatorSet.ScaleImage(ho_Image, out ho_ImageScaled, 1, 140 - hv_maxGrayValue[0]);

                ho_Region.Dispose();
                HOperatorSet.Threshold(ho_ImageScaled, out ho_Region, 80, 255);
                ho_RegionFillUp.Dispose();
                HOperatorSet.ShapeTrans(ho_Region, out ho_RegionFillUp, "convex");
                //HOperatorSet.FillUp(ho_Region, out ho_RegionFillUp);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_RegionFillUp, out ho_RegionErosion, Para.WhiteEr);
                //ho_RegionDifference.Dispose();
                //HOperatorSet.Difference(ho_RegionFillUp, ho_RegionErosion, out ho_RegionDifference
                //    );
                //ho_ImageReduced.Dispose();
                //HOperatorSet.ReduceDomain(ho_Image, ho_RegionDifference, out ho_ImageReduced);
                //HOperatorSet.Intensity(ho_RegionDifference, ho_ImageReduced, out hv_Mean, out hv_Deviation);
                //hv_meanResult = hv_Mean * 1.6;
                //if ((int)(new HTuple(hv_meanResult.TupleGreater(240))) != 0)
                //{
                //    hv_meanResult = 240;
                //}
                //ho_Region1.Dispose();
                //HOperatorSet.Threshold(ho_ImageReduced, out ho_Region1, hv_meanResult, 255);
                //ho_ConnectedRegions.Dispose();
                //HOperatorSet.Connection(ho_Region1, out ho_ConnectedRegions);
                //ho_SelectedRegions.Dispose();
                //HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area",
                //    "and", hv_area_Pixel, 99999);
                //HOperatorSet.CountObj(ho_SelectedRegions, out hv_Number);
                //if ((int)(new HTuple(hv_Number.TupleGreater(0))) != 0)
                //{
                //    OTemp[SP_O] = ho_EmptyRegion.CopyObj(1, -1);
                //    SP_O++;
                //    ho_EmptyRegion.Dispose();
                //    HOperatorSet.ConcatObj(OTemp[SP_O - 1], ho_SelectedRegions, out ho_EmptyRegion
                //        );
                //    OTemp[SP_O - 1].Dispose();
                //    SP_O = 0;
                //}
                ho_ImageReduced1.Dispose();
                HOperatorSet.ReduceDomain(ho_Image, ho_RegionErosion, out ho_ImageReduced1);
                ho_ImageEmphasize.Dispose();
                HOperatorSet.Emphasize(ho_ImageReduced1, out ho_ImageEmphasize, 11, 11, 1);
                ho_Region2.Dispose();
                HOperatorSet.Threshold(ho_ImageReduced1, out ho_Region2, 225, 255);
                //HOperatorSet.VarThreshold(ho_ImageEmphasize, out ho_Region2, hv_Width, hv_Height,
                //    0.2, 100, "light");
                ho_ConnectedRegions1.Dispose();
                HOperatorSet.Connection(ho_Region2, out ho_ConnectedRegions1);
                ho_SelectedRegions1.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions1, "area",
                    "and", hv_area_Pixel, 999999);
                HOperatorSet.CountObj(ho_SelectedRegions1, out hv_Number1);
                if ((int)(new HTuple(hv_Number1.TupleGreater(0))) != 0)
                {
                    OTemp[SP_O] = ho_EmptyRegion.CopyObj(1, -1);
                    SP_O++;
                    ho_EmptyRegion.Dispose();
                    HOperatorSet.ConcatObj(OTemp[SP_O - 1], ho_SelectedRegions1, out ho_EmptyRegion
                        );
                    OTemp[SP_O - 1].Dispose();
                    SP_O = 0;
                }
                // HOperatorSet.CountObj(ho_EmptyRegion, out hv_Number2);
                ho_SortRegion.Dispose();
                HOperatorSet.SortRegion(ho_SelectedRegions1, out ho_SortRegion, "first_point", "true", "column");
                hv_NumDot = hv_Number1.Clone();
                if (hv_NumDot.I > 5)
                {
                    WhiteParaReturn.whiteCounts = 5;
                }
                else

                    WhiteParaReturn.whiteCounts = hv_NumDot.I;
                if ((int)(new HTuple(hv_NumDot.TupleGreater(0))) != 0)
                {
                    ho_RegionDilation.Dispose();
                    HOperatorSet.DilationCircle(ho_SortRegion, out ho_RegionDilation, 5);
                    HOperatorSet.SmallestCircle(ho_RegionDilation, out hv_Row, out hv_Column, out hv_Radius);
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    ho_ImageResult.Dispose();
                    HOperatorSet.PaintRegion(ho_Circle, ho_Image, out ho_ImageResult, 0, "margin");

                    string fileName = ImagePath + "\\" + barcode + ".bmp";

                    HOperatorSet.WriteImage(ho_ImageResult, "bmp", 0, fileName);

                    string fileName1 = ImagePath + "\\" + "Original" + barcode + ".bmp";

                    HOperatorSet.WriteImage(ho_Image, "bmp", 0, fileName1);
                    WhiteParaReturn.whiteImage = ho_ImageResult;

                    if (hv_NumDot.I > 5)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            ho_SelectObjregion.Dispose();
                            HOperatorSet.SelectObj(ho_SortRegion, out ho_SelectObjregion, j + 1);
                            HOperatorSet.AreaCenter(ho_SelectObjregion, out hv_Area1, out hv_RowY, out hv_ColumnX);
                            WhiteParaReturn.whiteX[j] = (float)Math.Round(hv_ColumnX.D * pixel_MM, 3);
                            WhiteParaReturn.whiteY[j] = (float)Math.Round(hv_RowY.D * pixel_MM, 3);
                            WhiteParaReturn.whiteArea[j] = (float)Math.Round(hv_Area1.D * pixel_MM * pixel_MM, 3);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < hv_NumDot.I; j++)
                        {
                            ho_SelectObjregion.Dispose();
                            HOperatorSet.SelectObj(ho_SortRegion, out ho_SelectObjregion, j + 1);
                            HOperatorSet.AreaCenter(ho_SelectObjregion, out hv_Area1, out hv_RowY, out hv_ColumnX);
                            WhiteParaReturn.whiteX[j] = (float)Math.Round(hv_ColumnX.D * pixel_MM, 3);
                            WhiteParaReturn.whiteY[j] = (float)Math.Round(hv_RowY.D * pixel_MM, 3);
                            WhiteParaReturn.whiteArea[j] = (float)Math.Round(hv_Area1.D * pixel_MM * pixel_MM, 3);
                        }
                    }
                }

                ho_SelectObjregion.Dispose();
                ho_EmptyRegion.Dispose();
                ho_Region.Dispose();
                ho_RegionFillUp.Dispose();
                ho_RegionErosion.Dispose();
                ho_RegionDifference.Dispose();
                ho_ImageReduced.Dispose();
                ho_Region1.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions.Dispose();
                ho_ImageReduced1.Dispose();
                ho_ImageEmphasize.Dispose();
                ho_Region2.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions1.Dispose();
                ho_RegionDilation.Dispose();
                ho_Circle.Dispose();
                // ho_ImageResult.Dispose();

                ho_Domain11.Dispose();
                ho_ImageScaled.Dispose();
                ho_SortRegion.Dispose();
            }
            catch
            { }
            return WhiteParaReturn;
        }
        /// <summary>
        /// 黑点检测20171010
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="ho_Image"></param>
        /// <param name="pixel_MM"></param>
        /// <returns></returns>
        public BlackParaList BlackDotInspect(string barcode, string modle, HObject ho_Image, double pixel_MM, string ImagePath)
        {

            BlackParaList blackParaReturn = new BlackParaList();
            // Local iconic variables 
            HObject ho_Domain11, ho_ImageScaled;
            HObject ho_Regions, ho_RegionFillUp, ho_SelectObjregion;
            HObject ho_RegionErosion, ho_ImageReduced1, ho_ImageEmphasize;
            HObject ho_Region2, ho_ConnectedRegions, ho_SelectedRegions;
            HObject ho_RegionDilation = null, ho_Circle = null, ho_ImageResult = null;

            // Local control variables 
            HTuple hv_AbsoluteHisto, hv_RelativeHisto;
            HTuple hv_Max, hv_maxGrayValue;
            HTuple hv_NumDot = null, hv_Name = null, hv_Width = null, hv_RowY = null, hv_ColumnX = null, hv_Area1 = null;
            HTuple hv_Height = null, hv_radius_MM = null, hv_radius_Pixel = null;
            HTuple hv_area_Pixel = null, hv_Number = null, hv_Row = new HTuple();
            HTuple hv_Column = new HTuple(), hv_Radius = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_SelectObjregion);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            HOperatorSet.GenEmptyObj(out ho_ImageEmphasize);
            HOperatorSet.GenEmptyObj(out ho_Region2);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.GenEmptyObj(out ho_ImageResult);

            HOperatorSet.GenEmptyObj(out ho_Domain11);
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            hv_NumDot = 0;
            hv_Name = barcode;

            
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            ho_Domain11.Dispose();
            HOperatorSet.GetDomain(ho_Image, out ho_Domain11);
            HOperatorSet.GrayHisto(ho_Domain11, ho_Image, out hv_AbsoluteHisto, out hv_RelativeHisto);
            HOperatorSet.TupleMax(hv_AbsoluteHisto, out hv_Max);
            hv_maxGrayValue = hv_AbsoluteHisto.TupleFind(hv_Max);
            ho_ImageScaled.Dispose();
            HOperatorSet.ScaleImage(ho_Image, out ho_ImageScaled, 1, 140 - hv_maxGrayValue[0]);
            
            ho_ImageEmphasize.Dispose();
            HOperatorSet.Emphasize(ho_ImageScaled, out ho_ImageEmphasize, 39, 39, 2);
            ho_Regions.Dispose();
            HOperatorSet.Threshold(ho_ImageEmphasize, out ho_Regions, 50, 232);
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_Regions, out ho_RegionFillUp);
            ho_RegionErosion.Dispose();
            HOperatorSet.ErosionCircle(ho_RegionFillUp, out ho_RegionErosion, 1);
            ho_ImageReduced1.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageEmphasize, ho_RegionErosion, out ho_ImageReduced1);

            ho_Region2.Dispose();
            HOperatorSet.VarThreshold(ho_ImageReduced1, out ho_Region2, hv_Width, hv_Height,
                2, 100, "dark");
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Region2, out ho_ConnectedRegions);
            hv_radius_MM = pixel_MM;
            hv_radius_Pixel = 20 / hv_radius_MM;
            hv_area_Pixel = 0.005/ (pixel_MM * pixel_MM) ;
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area",
                "and", hv_area_Pixel, 99999);
            HOperatorSet.CountObj(ho_SelectedRegions, out hv_Number);
            hv_NumDot = hv_Number.Clone();
            if (hv_NumDot.I > 5)
            {
                blackParaReturn.blackCounts = 5;
            }
            else
            blackParaReturn.blackCounts = hv_NumDot.I;
            if ((int)(new HTuple(hv_NumDot.TupleGreater(0))) != 0)
            {
                ho_RegionDilation.Dispose();
                HOperatorSet.DilationCircle(ho_SelectedRegions, out ho_RegionDilation, 5);
                HOperatorSet.SmallestCircle(ho_RegionDilation, out hv_Row, out hv_Column, out hv_Radius);
                ho_Circle.Dispose();
                HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                ho_ImageResult.Dispose();
                HOperatorSet.PaintRegion(ho_Circle, ho_Image, out ho_ImageResult, 0, "margin");
                string fileName = ImagePath + "\\" + barcode + ".bmp";
                HOperatorSet.WriteImage(ho_ImageResult, "bmp", 0, fileName);
                string fileName1 = ImagePath + "\\" + "Original" + barcode + ".bmp";
                HOperatorSet.WriteImage(ho_Image, "bmp", 0, fileName1);
                if (hv_NumDot.I > 5)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        ho_SelectObjregion.Dispose();
                        HOperatorSet.SelectObj(ho_SelectedRegions, out ho_SelectObjregion, j + 1);
                        HOperatorSet.AreaCenter(ho_SelectObjregion, out hv_Area1, out hv_RowY, out hv_ColumnX);
                        blackParaReturn.blackX[j] = (float)Math.Round(hv_ColumnX.D * pixel_MM, 3);
                        blackParaReturn.blackY[j] = (float)Math.Round(hv_RowY.D * pixel_MM, 3);
                        blackParaReturn.blackArea[j] = (float)Math.Round(hv_Area1.D * pixel_MM * pixel_MM, 3);

                    }
                }

                else
                {
                    for (int j = 0; j < hv_NumDot.I; j++)
                    {
                        ho_SelectObjregion.Dispose();
                        HOperatorSet.SelectObj(ho_SelectedRegions, out ho_SelectObjregion, j + 1);
                        HOperatorSet.AreaCenter(ho_SelectObjregion, out hv_Area1, out hv_RowY, out hv_ColumnX);
                        blackParaReturn.blackX[j] =(float) Math.Round(hv_ColumnX.D * pixel_MM, 3);
                        blackParaReturn.blackY[j] =(float) Math.Round(hv_RowY.D * pixel_MM, 3);
                        blackParaReturn.blackArea[j] = (float)Math.Round(hv_Area1.D * pixel_MM * pixel_MM, 3);

                    }
                }


            }

            ho_SelectObjregion.Dispose();
            ho_Regions.Dispose();
            ho_RegionFillUp.Dispose();
            ho_RegionErosion.Dispose();
            ho_ImageReduced1.Dispose();
            ho_ImageEmphasize.Dispose();
            ho_Region2.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_RegionDilation.Dispose();
            ho_Circle.Dispose();
            ho_ImageResult.Dispose();

            ho_Domain11.Dispose();
            ho_ImageScaled.Dispose();
            return blackParaReturn;
        }

        public BlackParaList BlackDotInspectA(string barcode, string modle, HObject ho_Image, double pixel_MM, string ImagePath)
        {

            BlackParaList blackParaReturn = new BlackParaList();
            try
            {
                // Local iconic variables 
                HObject ho_Domain11, ho_ImageScaled;
                HObject ho_Regions, ho_RegionFillUp, ho_SelectObjregion;
                HObject ho_RegionErosion, ho_ImageReduced1, ho_ImageEmphasize;
                HObject ho_Region2, ho_ConnectedRegions, ho_SelectedRegions;
                HObject ho_RegionDilation = null, ho_Circle = null, ho_ImageResult = null;
                HObject ho_SortRegion;
                HObject ho_Connection1;
                HOperatorSet.GenEmptyObj(out ho_Connection1);

                HObject ho_SelectShape1;
                HOperatorSet.GenEmptyObj(out ho_SelectShape1);

                // Local control variables 
                HTuple hv_AbsoluteHisto, hv_RelativeHisto;
                HTuple hv_Max, hv_maxGrayValue;
                HTuple hv_NumDot = null, hv_Name = null, hv_Width = null, hv_RowY = null, hv_ColumnX = null, hv_Area1 = null;
                HTuple hv_Height = null, hv_radius_MM = null, hv_radius_Pixel = null;
                HTuple hv_area_Pixel = null, hv_Number = null, hv_Row = new HTuple();
                HTuple hv_Column = new HTuple(), hv_Radius = new HTuple();
                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_SelectObjregion);
                HOperatorSet.GenEmptyObj(out ho_Regions);
                HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
                HOperatorSet.GenEmptyObj(out ho_RegionErosion);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
                HOperatorSet.GenEmptyObj(out ho_ImageEmphasize);
                HOperatorSet.GenEmptyObj(out ho_Region2);
                HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
                HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
                HOperatorSet.GenEmptyObj(out ho_RegionDilation);
                HOperatorSet.GenEmptyObj(out ho_Circle);
                HOperatorSet.GenEmptyObj(out ho_ImageResult);

                HOperatorSet.GenEmptyObj(out ho_Domain11);
                HOperatorSet.GenEmptyObj(out ho_ImageScaled);
                HOperatorSet.GenEmptyObj(out ho_SortRegion);
                hv_NumDot = 0;
                hv_Name = barcode;


                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                ho_Domain11.Dispose();
                HOperatorSet.GetDomain(ho_Image, out ho_Domain11);
                HOperatorSet.GrayHisto(ho_Domain11, ho_Image, out hv_AbsoluteHisto, out hv_RelativeHisto);
                HOperatorSet.TupleMax(hv_AbsoluteHisto, out hv_Max);
                hv_maxGrayValue = hv_AbsoluteHisto.TupleFind(hv_Max);
                ho_ImageScaled.Dispose();
                HOperatorSet.ScaleImage(ho_Image, out ho_ImageScaled, 1, 140 - hv_maxGrayValue[0]);

                ho_ImageEmphasize.Dispose();
                HOperatorSet.Emphasize(ho_ImageScaled, out ho_ImageEmphasize, 39, 39, 2);
                ho_Regions.Dispose();
                HOperatorSet.Threshold(ho_ImageEmphasize, out ho_Regions, 100, 255);
                ho_Connection1.Dispose();
                HOperatorSet.Connection(ho_Regions, out ho_Connection1);
                ho_SelectShape1.Dispose();
                HOperatorSet.SelectShape(ho_Connection1, out ho_SelectShape1, "area",
                    "and", 2000, 9999999);
                ho_RegionFillUp.Dispose();
                //HOperatorSet.ShapeTrans(ho_SelectShape1, out ho_RegionFillUp, "convex");
                HOperatorSet.FillUp(ho_SelectShape1, out ho_RegionFillUp);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_RegionFillUp, out ho_RegionErosion, Para.BlackEr);
                ho_ImageReduced1.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageEmphasize, ho_RegionErosion, out ho_ImageReduced1);

                ho_Region2.Dispose();
                HOperatorSet.VarThreshold(ho_ImageReduced1, out ho_Region2, hv_Width, hv_Height,
                    2, 100, "dark");
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Region2, out ho_ConnectedRegions);
                hv_radius_MM = pixel_MM;
                hv_radius_Pixel = 20 / hv_radius_MM;
                hv_area_Pixel = 0.005 / (pixel_MM * pixel_MM);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area",
                    "and", hv_area_Pixel, 9999999);
                HOperatorSet.CountObj(ho_SelectedRegions, out hv_Number);
                ho_SortRegion.Dispose();
                HOperatorSet.SortRegion(ho_SelectedRegions, out ho_SortRegion, "first_point", "true", "column");
                hv_NumDot = hv_Number.Clone();
                if (hv_NumDot.I > 5)
                {
                    blackParaReturn.blackCounts = 5;
                }
                else
                    blackParaReturn.blackCounts = hv_NumDot.I;
                if ((int)(new HTuple(hv_NumDot.TupleGreater(0))) != 0)
                {
                    ho_RegionDilation.Dispose();
                    HOperatorSet.DilationCircle(ho_SelectedRegions, out ho_RegionDilation, 5);
                    HOperatorSet.SmallestCircle(ho_RegionDilation, out hv_Row, out hv_Column, out hv_Radius);
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    ho_ImageResult.Dispose();
                    HOperatorSet.PaintRegion(ho_Circle, ho_Image, out ho_ImageResult, 0, "margin");
                    string fileName = ImagePath + "\\" + barcode + ".bmp";
                    HOperatorSet.WriteImage(ho_ImageResult, "bmp", 0, fileName);
                    string fileName1 = ImagePath + "\\" + "Original" + barcode + ".bmp";
                    HOperatorSet.WriteImage(ho_Image, "bmp", 0, fileName1);
                    blackParaReturn.blackImage = ho_ImageResult;
                    if (hv_NumDot.I > 5)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            ho_SelectObjregion.Dispose();
                            HOperatorSet.SelectObj(ho_SortRegion, out ho_SelectObjregion, j + 1);
                            HOperatorSet.AreaCenter(ho_SelectObjregion, out hv_Area1, out hv_RowY, out hv_ColumnX);
                            blackParaReturn.blackX[j] = (float)Math.Round(hv_ColumnX.D * pixel_MM, 3);
                            blackParaReturn.blackY[j] = (float)Math.Round(hv_RowY.D * pixel_MM, 3);
                            blackParaReturn.blackArea[j] = (float)Math.Round(hv_Area1.D * pixel_MM * pixel_MM, 3);

                        }
                    }

                    else
                    {
                        for (int j = 0; j < hv_NumDot.I; j++)
                        {
                            ho_SelectObjregion.Dispose();
                            HOperatorSet.SelectObj(ho_SortRegion, out ho_SelectObjregion, j + 1);
                            HOperatorSet.AreaCenter(ho_SelectObjregion, out hv_Area1, out hv_RowY, out hv_ColumnX);
                            blackParaReturn.blackX[j] = (float)Math.Round(hv_ColumnX.D * pixel_MM, 3);
                            blackParaReturn.blackY[j] = (float)Math.Round(hv_RowY.D * pixel_MM, 3);
                            blackParaReturn.blackArea[j] = (float)Math.Round(hv_Area1.D * pixel_MM * pixel_MM, 3);

                        }
                    }


                }
                ho_SortRegion.Dispose();
                ho_SelectObjregion.Dispose();
                ho_Regions.Dispose();
                ho_RegionFillUp.Dispose();
                ho_RegionErosion.Dispose();
                ho_ImageReduced1.Dispose();
                ho_ImageEmphasize.Dispose();
                ho_Region2.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions.Dispose();
                ho_RegionDilation.Dispose();
                ho_Circle.Dispose();
                //ho_ImageResult.Dispose();

                ho_Domain11.Dispose();
                ho_ImageScaled.Dispose();
            }
            catch
            { }
            return blackParaReturn;
        }

        public RectData FindRectNew(HObject ho_Image, double calival)
        {
            RectData myResult = new RectData();
            myResult.Found = false;

            try
            {
                HTuple hv_Width, hv_Height, hv_Mean, hv_Deviation;
                HObject ho_Rect, ho_EmptyImage, hv_selectedShape, ho_ConnectedRegion, ho_ShapeModelRegion, hV_reducedImg;
                HObject ho_ShapeModel;
                HTuple hv_RowCheck = new HTuple(), hv_ColumnCheck = new HTuple();
                HTuple hv_AngleCheck = new HTuple(), hv_ScaleCheck = new HTuple();
                HTuple hv_Score = new HTuple(), hv_j = new HTuple(), hv_MovementOfObject = new HTuple();
                HTuple hv_MoveAndScaleOfObject = new HTuple();
                hv_Mean = new HTuple(); hv_Deviation = new HTuple();

                HTuple hv_Y, hv_X, hv_AngRad, hv_HalfWidth, hv_HalfLength, hv_PtOrder;
                HObject ho_Rectangle;
                hv_Width = new HTuple();
                hv_Height = new HTuple();
                hv_Y = new HTuple();
                hv_X = new HTuple();
                hv_AngRad = new HTuple();
                hv_HalfWidth = new HTuple();
                hv_HalfLength = new HTuple();
                hv_PtOrder = new HTuple();
                HTuple hv_Number = new HTuple();

                //Init Variable
                HOperatorSet.GenEmptyObj(out ho_Rect);
                HOperatorSet.GenEmptyObj(out ho_EmptyImage);
                HOperatorSet.GenEmptyObj(out hv_selectedShape);
                HOperatorSet.GenEmptyObj(out ho_ConnectedRegion);
                HOperatorSet.GenEmptyObj(out ho_ShapeModelRegion);
                HOperatorSet.GenEmptyObj(out ho_ShapeModel);
                HOperatorSet.GenEmptyObj(out hV_reducedImg);
                //HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                //HOperatorSet.SetWindowAttr("background_color", "black");

                HOperatorSet.Threshold(ho_Image, out ho_ShapeModelRegion, 53, 255);
                HOperatorSet.Connection(ho_ShapeModelRegion, out ho_ConnectedRegion);
                //HOperatorSet.CountObj(ho_ConnectedRegion, out hv_Number);

                HOperatorSet.SelectShape(ho_ConnectedRegion, out hv_selectedShape, "area", "and", 150000, 330000); //Alvin 28July16 2500
                HOperatorSet.SmallestRectangle2(hv_selectedShape, out hv_Y, out hv_X, out hv_AngRad, out hv_HalfWidth, out hv_HalfLength);


                if (hv_X.Length != 0)
                {
                    //20161224@Brando
                    int i = 0;
                    HTuple h_temp = 0;
                    for (int t = 0; t < hv_X.Length; t++)
                    {
                        if (hv_Y[t] > h_temp)
                        {
                            h_temp = hv_Y[t];
                            i = t;
                        }
                    }
                    HOperatorSet.GenRectangle2(out ho_Rectangle, hv_Y[i].D, hv_X[i].D, hv_AngRad[i].D, hv_HalfWidth[i].D, hv_HalfLength[i].D);
                    HOperatorSet.Intensity(ho_Rectangle, ho_Image, out hv_Mean, out hv_Deviation);
                    myResult.Means = hv_Mean.D;
                    //20161027
                    HOperatorSet.ReduceDomain(ho_Image, ho_Rectangle, out hV_reducedImg);
                    HOperatorSet.CropDomain(hV_reducedImg, out myResult.InspectedImage);
                    if ((myResult.Means > 90) && (myResult.Means < 160))//20161101 FROM 220 TO 230
                    {
                        myResult.Found = true;
                        myResult.X = hv_X[i].D;
                        myResult.Y = hv_Y[i].D;
                        myResult.Angle = hv_AngRad[i].D;
                        myResult.Width = hv_HalfWidth[i].D * 2;
                        myResult.Height = hv_HalfLength[i].D * 2;
                        myResult.isCircle = false;
                        //myResult.rect_border = ho_Rectangle;//hv_selectedShape;

                        if (Para.isWidth818)
                        {
                            if ((myResult.X < 200) || (myResult.X - hv_HalfWidth[i].D) < (0.2 / calival) || (myResult.X + hv_HalfWidth[i].D + (0.2 / calival)) > 2583 || (myResult.Height < 150) || (myResult.Y < 300) || (myResult.Width < (7.721 / calival)) || (myResult.Width > (8.639 / calival)) || (myResult.Angle * 180 / 3.14 > 10) || (myResult.Angle * 180 / 3.14 < -10))
                                myResult.Found = false;
                        }
                        else
                        {
                            if ((myResult.X < 200) || (myResult.X - hv_HalfWidth[i].D) < (0.2 / calival) || (myResult.X + hv_HalfWidth[i].D + (0.2 / calival)) > 2583 || (myResult.Height < 150) || (myResult.Y < 300) || (myResult.Width < (8.74 / calival)) || (myResult.Width > 9.66 / calival) || (myResult.Angle * 180 / 3.14 > 10) || (myResult.Angle * 180 / 3.14 < -10))
                                myResult.Found = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
            return myResult;
        }
        public RectData FindCircle(HObject ho_Image)
        {
            RectData myResult = new RectData();
            HTuple hv_Width, hv_Height, hv_Mean, hv_Deviation;
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            HOperatorSet.SetWindowAttr("background_color", "black");
            HTuple hv_RadiusCircle, hv_SizeSynthImage;
            HObject ho_Circle, ho_EmptyImage, ho_SyntheticModelImage, ho_ShapeModelImage, ho_ShapeModelRegion;
            HObject ho_ShapeModel;
            HTuple hv_ModelID, hv_RowCheck = new HTuple(), hv_ColumnCheck = new HTuple();
            HTuple hv_AngleCheck = new HTuple(), hv_ScaleCheck = new HTuple();
            HTuple hv_Score = new HTuple(), hv_j = new HTuple(), hv_MovementOfObject = new HTuple();
            HTuple hv_MoveAndScaleOfObject = new HTuple();
            hv_Mean = new HTuple(); hv_Deviation = new HTuple();
            HObject hV_reducedImg;

            //Init Variable
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.GenEmptyObj(out ho_EmptyImage);
            HOperatorSet.GenEmptyObj(out ho_SyntheticModelImage);
            HOperatorSet.GenEmptyObj(out ho_ShapeModelImage);
            HOperatorSet.GenEmptyObj(out ho_ShapeModelRegion);
            HOperatorSet.GenEmptyObj(out ho_ShapeModel);
            HOperatorSet.GenEmptyObj(out hV_reducedImg);
            //step 1: Generate Default Circle Shape
            hv_RadiusCircle = 190;
            hv_SizeSynthImage = (2 * hv_RadiusCircle) + 10;
            ho_Circle.Dispose();
            HOperatorSet.GenEllipseContourXld(out ho_Circle, hv_SizeSynthImage / 2, hv_SizeSynthImage / 2,
                0, hv_RadiusCircle, hv_RadiusCircle, 0, 6.28318, "positive", 1.5);

            //step 2: create an image and insert the XLD        
            ho_EmptyImage.Dispose();
            HOperatorSet.GenImageConst(out ho_EmptyImage, "byte", hv_SizeSynthImage, hv_SizeSynthImage);
            ho_SyntheticModelImage.Dispose();
            HOperatorSet.PaintXld(ho_Circle, ho_EmptyImage, out ho_SyntheticModelImage, 128);

            //step 3: create the model
            ho_ShapeModelImage.Dispose();
            ho_ShapeModelRegion.Dispose();
            HOperatorSet.InspectShapeModel(ho_SyntheticModelImage, out ho_ShapeModelImage, out ho_ShapeModelRegion, 1, 30);
            HOperatorSet.CreateScaledShapeModel(ho_SyntheticModelImage, "auto", 0, 0, 0.01,
                        0.8, 1.2, "auto", "none", "use_polarity", 30, 10, out hv_ModelID);
            ho_ShapeModel.Dispose();
            HOperatorSet.GetShapeModelContours(out ho_ShapeModel, hv_ModelID, 1);



            //Step 4 : Find the Circle
            HOperatorSet.FindScaledShapeModel(ho_Image, hv_ModelID, 0, 0, 0.4,
                1.6, 0.6, 0, 0.5, "least_squares", 3, 0, out hv_RowCheck, out hv_ColumnCheck,
                out hv_AngleCheck, out hv_ScaleCheck, out hv_Score);

            List<DPoint> CirCtr = new List<DPoint>();

            if ((hv_Score.Length != 0) && (hv_RadiusCircle > 100))
            {
                for (int i = 0; i < hv_Score.Length; i++)
                {

                    CirCtr.Add(new DPoint(hv_ColumnCheck[i].D, hv_RowCheck[i].D));

                    HObject ho_Rect = new HObject();
                    HOperatorSet.GenCircle(out ho_Rect, hv_RowCheck[i].D, hv_ColumnCheck[i].D, hv_RadiusCircle * hv_ScaleCheck[i].D);
                    HOperatorSet.Intensity(ho_Rect, ho_Image, out hv_Mean, out hv_Deviation);
                    //myResult.Means = hv_Mean.D;
                    //HOperatorSet.RegionToLabel(ho_Rect, out myResult.InspectedImage, "byte", hv_RadiusCircle * hv_ScaleCheck[i].D * 2, hv_RadiusCircle * hv_ScaleCheck[i].D * 2);

                    //string DataFileName = @"D:\";
                    //string temp1 = DataFileName + "Test";
                    //HOperatorSet.WriteImage(myResult.InspectedImage, "bmp", 0, temp1);

                    if ((hv_Mean.D > 100))//(myResult.Means < 220) && 
                    {
                        HOperatorSet.ReduceDomain(ho_Image, ho_Rect, out hV_reducedImg);
                        HOperatorSet.CropDomain(hV_reducedImg, out myResult.InspectedImage);
                        myResult.Found = true;
                        myResult.X = hv_ColumnCheck[i].D;
                        myResult.Y = hv_RowCheck[i].D;
                        myResult.Radius = hv_RadiusCircle * hv_ScaleCheck[i].D;
                        myResult.isCircle = true;
                        myResult.Means = hv_Mean.D;
                        //break;
                    }
                    //myResult.InspectedImage = ho_Rect;
                }
            }


            //if (myResult.Found)
            //{
            //    if (CirCtr.Count == 2)
            //    {
            //        myResult.Angle = Helper.GetRadianAngleBetween(CirCtr[0], CirCtr[1]);
            //    }
            //}
            return myResult;

        }
        public void disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem, HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {


            // Local control variables 

            HTuple hv_Red, hv_Green, hv_Blue, hv_Row1Part;
            HTuple hv_Column1Part, hv_Row2Part, hv_Column2Part, hv_RowWin;
            HTuple hv_ColumnWin, hv_WidthWin, hv_HeightWin, hv_MaxAscent;
            HTuple hv_MaxDescent, hv_MaxWidth, hv_MaxHeight, hv_R1 = new HTuple();
            HTuple hv_C1 = new HTuple(), hv_FactorRow = new HTuple(), hv_FactorColumn = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Index = new HTuple(), hv_Ascent = new HTuple();
            HTuple hv_Descent = new HTuple(), hv_W = new HTuple(), hv_H = new HTuple();
            HTuple hv_FrameHeight = new HTuple(), hv_FrameWidth = new HTuple();
            HTuple hv_R2 = new HTuple(), hv_C2 = new HTuple(), hv_DrawMode = new HTuple();
            HTuple hv_Exception = new HTuple(), hv_CurrentColor = new HTuple();

            HTuple hv_Color_COPY_INP_TMP = hv_Color.Clone();
            HTuple hv_Column_COPY_INP_TMP = hv_Column.Clone();
            HTuple hv_Row_COPY_INP_TMP = hv_Row.Clone();
            HTuple hv_String_COPY_INP_TMP = hv_String.Clone();

            //prepare window
            HOperatorSet.GetRgb(hv_WindowHandle, out hv_Red, out hv_Green, out hv_Blue);
            HOperatorSet.GetPart(hv_WindowHandle, out hv_Row1Part, out hv_Column1Part, out hv_Row2Part,
                out hv_Column2Part);
            HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_RowWin, out hv_ColumnWin,
                out hv_WidthWin, out hv_HeightWin);
            HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_HeightWin - 1, hv_WidthWin - 1);
            //
            //default settings
            if ((int)(new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Row_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Column_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Color_COPY_INP_TMP.TupleEqual(new HTuple()))) != 0)
            {
                hv_Color_COPY_INP_TMP = "";
            }
            //
            hv_String_COPY_INP_TMP = ((("" + hv_String_COPY_INP_TMP) + "")).TupleSplit("\n");
            //
            //Estimate extentions of text depending on font size.
            HOperatorSet.GetFontExtents(hv_WindowHandle, out hv_MaxAscent, out hv_MaxDescent,
                out hv_MaxWidth, out hv_MaxHeight);
            if ((int)(new HTuple(hv_CoordSystem.TupleEqual("window"))) != 0)
            {
                hv_R1 = hv_Row_COPY_INP_TMP.Clone();
                hv_C1 = hv_Column_COPY_INP_TMP.Clone();
            }
            else
            {
                //transform image to window coordinates
                hv_FactorRow = (1.0 * hv_HeightWin) / ((hv_Row2Part - hv_Row1Part) + 1);
                hv_FactorColumn = (1.0 * hv_WidthWin) / ((hv_Column2Part - hv_Column1Part) + 1);
                hv_R1 = ((hv_Row_COPY_INP_TMP - hv_Row1Part) + 0.5) * hv_FactorRow;
                hv_C1 = ((hv_Column_COPY_INP_TMP - hv_Column1Part) + 0.5) * hv_FactorColumn;
            }
            //
            //display text box depending on text size
            if ((int)(new HTuple(hv_Box.TupleEqual("true"))) != 0)
            {
                //calculate box extents
                hv_String_COPY_INP_TMP = (" " + hv_String_COPY_INP_TMP) + " ";
                hv_Width = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    HOperatorSet.GetStringExtents(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                        hv_Index), out hv_Ascent, out hv_Descent, out hv_W, out hv_H);
                    hv_Width = hv_Width.TupleConcat(hv_W);
                }
                hv_FrameHeight = hv_MaxHeight * (new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    ));
                hv_FrameWidth = (((new HTuple(0)).TupleConcat(hv_Width))).TupleMax();
                hv_R2 = hv_R1 + hv_FrameHeight;
                hv_C2 = hv_C1 + hv_FrameWidth;
                //display rectangles
                HOperatorSet.GetDraw(hv_WindowHandle, out hv_DrawMode);
                HOperatorSet.SetDraw(hv_WindowHandle, "fill");
                HOperatorSet.SetColor(hv_WindowHandle, "light gray");
                HOperatorSet.DispRectangle1(hv_WindowHandle, hv_R1 + 3, hv_C1 + 3, hv_R2 + 3, hv_C2 + 3);
                HOperatorSet.SetColor(hv_WindowHandle, "white");
                HOperatorSet.DispRectangle1(hv_WindowHandle, hv_R1, hv_C1, hv_R2, hv_C2);
                HOperatorSet.SetDraw(hv_WindowHandle, hv_DrawMode);
            }
            else if ((int)(new HTuple(hv_Box.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Box";
                throw new HalconException(hv_Exception);
            }
            //Write text.
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                )) - 1); hv_Index = (int)hv_Index + 1)
            {
                hv_CurrentColor = hv_Color_COPY_INP_TMP.TupleSelect(hv_Index % (new HTuple(hv_Color_COPY_INP_TMP.TupleLength()
                    )));
                if ((int)((new HTuple(hv_CurrentColor.TupleNotEqual(""))).TupleAnd(new HTuple(hv_CurrentColor.TupleNotEqual(
                    "auto")))) != 0)
                {
                    HOperatorSet.SetColor(hv_WindowHandle, hv_CurrentColor);
                }
                else
                {
                    HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
                }
                hv_Row_COPY_INP_TMP = hv_R1 + (hv_MaxHeight * hv_Index);
                HOperatorSet.SetTposition(hv_WindowHandle, hv_Row_COPY_INP_TMP, hv_C1);
                HOperatorSet.WriteString(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                    hv_Index));
            }
            //reset changed window settings
            HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
            HOperatorSet.SetPart(hv_WindowHandle, hv_Row1Part, hv_Column1Part, hv_Row2Part,
                hv_Column2Part);

            return;
        }
    }
}
