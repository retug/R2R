using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RAMDATAACCESSLib;



namespace rebarBenderMulti
{
    public class RAMInfo
    {
        public static int GET_GRID_NAMES(string FileName)
        {
            RamDataAccess1 RAMDataAccess = new RAMDATAACCESSLib.RamDataAccess1();
            RAMDATAACCESSLib.IDBIO1 IDBI = (RAMDATAACCESSLib.IDBIO1)RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            //FW - HAD TO ADD Microsoft.CSharp Referecne to get this work, very werid???
            RAMDATAACCESSLib.IModel IModel = (RAMDATAACCESSLib.IModel)RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IModel_INT);

            //OPEN
            IDBI.LoadDataBase2(FileName, "1");
            IStories My_stories = IModel.GetStories();
            int My_story_count = My_stories.GetCount();

            //CLOSE           
            IDBI.CloseDatabase();
            return My_story_count;
        }


        public static List<string> GET_STORY_NAMES(string FileName)
        {
            RamDataAccess1 RAMDataAccess = new RAMDATAACCESSLib.RamDataAccess1();
            RAMDATAACCESSLib.IDBIO1 IDBI = (RAMDATAACCESSLib.IDBIO1)RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            RAMDATAACCESSLib.IModel IModel = (RAMDATAACCESSLib.IModel)RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IModel_INT);
            //OPEN
            IDBI.LoadDataBase2(FileName, "1");
            IStories My_stories = IModel.GetStories();
            int Story_Count = My_stories.GetCount();
            List<string> ListLine = new List<string>();
            for (int i = 0; i < Story_Count; i = i + 1)
            {
                string My_Story_Names = My_stories.GetAt(i).strLabel;
                ListLine.Add(My_Story_Names);
            }
            //CLOSE           
            IDBI.CloseDatabase();
            return ListLine;
        }

        public static List<string> GET_GRID_NAMES(string FileName, int StoryCount, ref List<RAMGrid> Grid_List, ref List<r2rPoint> Point_List, ref List<CustomLine> ramGridsGathered)
        {
            RamDataAccess1 RAMDataAccess = new RAMDATAACCESSLib.RamDataAccess1();
            RAMDATAACCESSLib.IDBIO1 IDBI = (RAMDATAACCESSLib.IDBIO1)RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            //dynamic IDBI = RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            RAMDATAACCESSLib.IModel IModel = (RAMDATAACCESSLib.IModel)RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IModel_INT);
            //dynamic IModel = RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IModel_INT);

            //OPEN
            IDBI.LoadDataBase2(FileName, "1");

            IStories My_stories = IModel.GetStories();
            int My_story_count = My_stories.GetCount();
            IStory My_Story = My_stories.GetAt(StoryCount);
            IFloorType My_Floor = My_Story.GetFloorType();

            //IGridSystem My_Grid_System = IModel.GetGridSystem(StoryCount);
            //double dxOffset = My_Grid_System.dXOffset;
            IDAArray IDMy_Gird_Systems = My_Floor.GetGridSystemIDArray();

            //retreives all GridSystems in the model, I would like to get it to the point where you just retreive on a per level basis
            // but we will proceed with this direction for now. It seems like IDAArray should do the trick but the code is confusing
            //We go from  the story of interest, to finding what "floor type" is on that story, to reteiving the "IDArray" for the 
            //floor of interest.

            List<double> Grid_Ordinates = new List<double>();
            List<string> Grid_Name = new List<string>();
            List<string> Grid_Axis = new List<string>();
            List<string> ListLine = new List<string>();

            List<double> Grid_Rotation = new List<double>();
            List<double> Grid_Offset_X = new List<double>();
            List<double> Grid_Offset_Y = new List<double>();


            //note, it is not well documented that each grid system has a property that is lUID, struggled to figure this part out.

            IGridSystems allGridSystems = IModel.GetGridSystems();

            for (int i = 0; i < allGridSystems.GetCount(); i = i + 1)
            {
                //this acts on a single grid system

                double My_Grid_Rotation = allGridSystems.GetAt(i).dRotation;
                double My_Grid_Offset_X = allGridSystems.GetAt(i).dXOffset / 12;
                double My_Grid_Offset_Y = allGridSystems.GetAt(i).dYOffset / 12;

                Grid_Rotation.Add(My_Grid_Rotation);
                Grid_Offset_X.Add(My_Grid_Offset_X);
                Grid_Offset_Y.Add(My_Grid_Offset_Y);


                List<double> My_Vector = new List<double>();
                My_Vector.Add(Math.Cos(My_Grid_Rotation * Math.PI / 180));
                My_Vector.Add(Math.Sin(My_Grid_Rotation * Math.PI / 180));
                My_Vector.Add(0);

                List<double> Ref_Point = new List<double>();

                Ref_Point.Add(My_Grid_Offset_X);
                Ref_Point.Add(My_Grid_Offset_Y);
                Ref_Point.Add(0);

                GlobalCoordinateSystem gcs = new GlobalCoordinateSystem(Ref_Point, My_Vector);

                int My_Grid_ID = allGridSystems.GetAt(i).lUID;
                IModelGrids My_Grids = allGridSystems.GetAt(i).GetGrids();


                int My_Grid_Count = My_Grids.GetCount();
                for (int j = 0; j < My_Grid_Count; j = j + 1)
                {

                    //retreives all grids within single grid system
                    double My_Grid_ORD = Math.Ceiling(My_Grids.GetAt(j).dCoordinate_Angle / 12);
                    int Is_Max = My_Grids.GetAt(j).bApplyMaxLimit;
                    int Is_Min = My_Grids.GetAt(j).bApplyMinLimit;
                    string My_Model_Grid_Names = My_Grids.GetAt(j).strLabel;
                    string My_Model_Grid_Axis = My_Grids.GetAt(j).eAxis.ToString();
                    string My_String_Cleanup1 = My_Model_Grid_Axis.Remove(0, 5);
                    //This is either X or Y based on cleaning up the response
                    string My_StringCleanup2 = My_String_Cleanup1.Remove(1);




                    double? My_Grid_Min = null;
                    double? My_Grid_Max = null;

                    if (Is_Max == 1 && Is_Min == 1)
                    {
                        My_Grid_Min = Math.Ceiling(My_Grids.GetAt(j).dMinLimitValue / 12);
                        My_Grid_Max = Math.Ceiling(My_Grids.GetAt(j).dMaxLimitValue / 12);

                        if (My_StringCleanup2 == "X")
                        {
                            r2rPoint My_Point1 = new r2rPoint(My_Grid_ORD, (double)My_Grid_Min, 0, false);
                            My_Point1.Convert_To_Global(gcs);
                            r2rPoint My_Point2 = new r2rPoint(My_Grid_ORD, (double)My_Grid_Max, 0, false);
                            My_Point2.Convert_To_Global(gcs);

                            Point startPoint = new Point(My_Point1.X, My_Point1.Y);
                            Point endPoint = new Point(My_Point2.X, My_Point2.Y);

                            Point_List.Add(My_Point1);
                            Point_List.Add(My_Point2);

                            CustomLine RAMGrid = new CustomLine(startPoint, endPoint, My_Model_Grid_Names);

                            ramGridsGathered.Add(RAMGrid);
                        }
                        else if (My_StringCleanup2 == "Y")
                        {
                            r2rPoint My_Point1 = new r2rPoint((double)My_Grid_Min, My_Grid_ORD, 0, false);
                            My_Point1.Convert_To_Global(gcs);
                            r2rPoint My_Point2 = new r2rPoint((double)My_Grid_Max, My_Grid_ORD, 0, false);
                            My_Point2.Convert_To_Global(gcs);

                            Point_List.Add(My_Point1);
                            Point_List.Add(My_Point2);

                            Point startPoint = new Point(My_Point1.X, My_Point1.Y);
                            Point endPoint = new Point(My_Point2.X, My_Point2.Y);

                            Point_List.Add(My_Point1);
                            Point_List.Add(My_Point2);

                            CustomLine RAMGrid = new CustomLine(startPoint, endPoint, My_Model_Grid_Names);

                            ramGridsGathered.Add(RAMGrid);
                        }
                    }


                    RAMGrid My_Grid = new RAMGrid();
                    My_Grid.Name = My_Model_Grid_Names;
                    My_Grid.Location = My_Grid_ORD;
                    My_Grid.Min = My_Grid_Min;
                    My_Grid.Max = My_Grid_Max;
                    My_Grid.U_V = My_StringCleanup2;
                    Grid_List.Add(My_Grid);
                    Grid_Ordinates.Add(My_Grid_ORD);
                    Grid_Name.Add(My_Model_Grid_Names);
                    Grid_Axis.Add(My_StringCleanup2);
                }
                ListLine.Add(My_Grid_ID.ToString());
            }

            IDBI.CloseDatabase();
            return ListLine;

        }
        public static void GET_BEAM_INFO(string RSSFileName, int storyValue, ref List<RAMBeam> BeamList)
        {
            RamDataAccess1 RAMDataAccess = new RAMDATAACCESSLib.RamDataAccess1();
            RAMDATAACCESSLib.IDBIO1 IDBI = (RAMDATAACCESSLib.IDBIO1)RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            //dynamic IDBI = RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            RAMDATAACCESSLib.IModel IModel = (RAMDATAACCESSLib.IModel)RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IModel_INT);
            //dynamic IModel = RAMDataAccess.GetInterfacePointerByEnum(EINTERFACES.IModel_INT);
            //OPEN
            IDBI.LoadDataBase2(RSSFileName, "1");
            IStories My_stories = IModel.GetStories();
            int My_story_count = My_stories.GetCount();
            IStory My_Story = My_stories.GetAt(storyValue);
            IBeams My_Beams = My_Story.GetBeams();
            int Beam_Count = My_Beams.GetCount(); //total number of beams

            List<string> BeamListSize = new List<string>();


            SCoordinate P1 = new SCoordinate();
            SCoordinate P2 = new SCoordinate();
            for (int i = 0; i < Beam_Count; i = i + 1)
            {
                string My_Beam_Size = My_Story.GetBeams().GetAt(i).strSectionLabel;
                My_Story.GetBeams().GetAt(i).GetCoordinates(EBeamCoordLoc.eBeamEnds, ref P1, ref P2);
                //start
                double P1x = P1.dXLoc;
                double P1y = P1.dYLoc;
                double P1z = P1.dZLoc;
                //end
                double P2x = P2.dXLoc;
                double P2y = P2.dYLoc;
                double P2z = P2.dZLoc;

                List<double> startCoord = new List<double> { P1x, P1y, P1z };
                List<double> endCoord = new List<double> { P2x, P2y, P2z };

                double Length = Math.Sqrt(Math.Pow((P2x - P1x), 2) + Math.Pow((P2y - P1y), 2)) / 12;

                RAMBeam myBeam = new RAMBeam(My_Beam_Size, startCoord, endCoord, Length);
                BeamList.Add(myBeam);
            }
            //CLOSE    
            IDBI.CloseDatabase();
        }
    }
}
