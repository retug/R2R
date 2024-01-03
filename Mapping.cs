using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace rebarBenderMulti
{
    public class Mapping
    {
        /*
         This while loop will begin by first taking a RAM strucutral beam and then comparing
         to every beam in revit at the specified floor. It takes tolerance as a user input.
         If the RAM Beam is mapped, which is determined by if the the two end points of a 
         RAM beam is within the given tolerance of a REvit beam, the RAM parameters are 
         mapped into the corresponding revit parameters.
         If a RAM Beam is not mapped, it will be flagged and drawn with a RED line for the CAD/engineer
         staff to review. This tool can be used to drive data into Revit and can be a QA/QC
         check to ensure Revit and RAM are coordinated        
         */
        public void MapRAMBeams(List<RAMBeam> ramBeamList, List<RevitBeam> revitBeamList, double tol) 
        {
            List<RevitBeam> remRevitBeamList = new List<RevitBeam>(revitBeamList);
            int i = 0;
            while (i < ramBeamList.Count)
            {
                //System.Windows.Point RAM_BEAM_point_1a = new System.Windows.Point(ramBeamList[i].StartPoint[0], ramBeamList[i].StartPoint[1]);
                System.Windows.Point RAM_BEAM_point_1a = ramBeamList[i].gloStartPoint;
                System.Windows.Point RAM_BEAM_point_2a = ramBeamList[i].gloEndPoint;

                System.Windows.Point RAM_BEAM_point_1b = ramBeamList[i].gloEndPoint;
                System.Windows.Point RAM_BEAM_point_2b = ramBeamList[i].gloStartPoint;
                int j = 0;
                bool mapped = false;
                while (!mapped)
                {
                    if (j < remRevitBeamList.Count)
                    {
                        // Get the start and end points of the structural framing element
                        Element RevitElement = remRevitBeamList[j].RevitElement;
                        LocationCurve locationCurve = (RevitElement.Location as LocationCurve);
                        // Get the start and end points of the structural framing element
                        XYZ startPoint = locationCurve.Curve.GetEndPoint(0);
                        XYZ endPoint = locationCurve.Curve.GetEndPoint(1);

                        System.Windows.Point REVIT_BEAM_point_1 = new System.Windows.Point(startPoint.X, -startPoint.Y);
                        System.Windows.Point REVIT_BEAM_point_2 = new System.Windows.Point(endPoint.X, -endPoint.Y);

                        //check for any order of beam orientation
                        double dist_1a = calc_distance(RAM_BEAM_point_1a, REVIT_BEAM_point_1);
                        double dist_2a = calc_distance(RAM_BEAM_point_2a, REVIT_BEAM_point_2);

                        double dist_1b = calc_distance(RAM_BEAM_point_1b, REVIT_BEAM_point_1);
                        double dist_2b = calc_distance(RAM_BEAM_point_2b, REVIT_BEAM_point_2);

                        //The beam has found a match in revit.
                        //Remove the beam from the possible matches in revit beam list
                        if ((dist_1a <= tol && dist_2a <= tol) || (dist_1b <= tol && dist_2b <= tol))
                        {
                            ramBeamList[i].Mapped = true;
                            // Find the index of the targetBeam in revitBeamList
                            
                            int index = revitBeamList.IndexOf(remRevitBeamList[j]);
                            revitBeamList[index].Mapped = true;
                            //IF THE BEAMS BETWEEN REVIT AND RAM DO NOT MATCH
                            if (ramBeamList[i].beamName.Text != revitBeamList[index].beamName.Text.ToUpper())   
                            {
                                ramBeamList[i].Comment = $"Revit beam: {revitBeamList[index].beamName.Text}";
                            }
                            mapped = true;
                            remRevitBeamList.RemoveAt(j);
                        }
                        else
                        {
                            j ++;
                        }
                    
                    }
                    else 
                    {
                        mapped = true;

                    }
                    
                }
                i++;
            }    
        }

        public double calc_distance(System.Windows.Point point1, System.Windows.Point point2)
        {
            double dist = Math.Pow(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2), 0.5);
            return dist;
        }
    }
}
