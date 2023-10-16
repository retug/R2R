using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace rebarBenderMulti
{
    public class r2rPoint : INotifyPropertyChanged
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public double U { get; set; }
        public double V { get; set; }
        public double W { get; set; }

        private bool isGlobal;

        public List<double> LocalCoords { get; set; }
        public List<double> GlobalCoords { get; set; }

        //Initialize the point class in either the global or local coordinate system thru isGlobal Flag

        public r2rPoint(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            isGlobal = true;
        }

        public r2rPoint(double U, double V, double W, bool isGlobal = false)
        {
            this.U = U;
            this.V = V;
            this.W = W;
            this.isGlobal = isGlobal;
        }

        public void Convert_To_Local(GlobalCoordinateSystem gcs)
        {
            double[] part1 = new double[] { X - gcs.RefPnt[0], Y - gcs.RefPnt[1], Z - gcs.RefPnt[2] };


            //the class will now have new attribute of local coordinates point.LocalCoords[0] = the X local coordinate system
            LocalCoords = new List<double>() { gcs.R_Inv[0, 0] * part1[0] + gcs.R_Inv[0, 1] * part1[1] + gcs.R_Inv[0, 2] * part1[2] ,
            gcs.R_Inv[1, 0] * part1[0] + gcs.R_Inv[1, 1] * part1[1] + gcs.R_Inv[1, 2] * part1[2],
            gcs.R_Inv[2, 0] * part1[0] + gcs.R_Inv[2, 1] * part1[1] + gcs.R_Inv[2, 2] * part1[2]};


            this.U = LocalCoords[0];
            this.V = LocalCoords[1];
            this.W = LocalCoords[2];
        }

        public void Convert_To_Global(GlobalCoordinateSystem gcs)
        {
            //this is the ref point
            double[] part1 = new double[] { U, V, W };


            //the class will now have new attribute of local coordinates point.LocalCoords[0] = the X local coordinate system
            GlobalCoords = new List<double>() { (gcs.R[0, 0] * U + gcs.R[0, 1] * V + gcs.R[0, 2] * W) + gcs.RefPnt[0],
            (gcs.R[1, 0] * U + gcs.R[1, 1] * V + gcs.R[1, 2] * W) + gcs.RefPnt[1],
            (gcs.R[2, 0] * U + gcs.R[2, 1] * V + gcs.R[2, 2] * W) + 0}; //unsure why this the way to do this. review in the future. should be the line below, without the 0
            //(globalCoords.R[2, 0] * X + globalCoords.R[2, 1] * Y + globalCoords.R[2, 2] * Z) + globalCoords.RefPnt[2]};

            this.X = GlobalCoords[0];
            this.Y = GlobalCoords[1];
            this.Z = GlobalCoords[2];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GlobalCoordinateSystem
    {
        public List<double> RefPnt { get; set; }
        public List<double> Vector { get; set; }
        public double hyp { get; set; }
        public double[,] R { get; set; }
        public string inverseMatrixText { get; set; }
        public Matrix<double> R_Matrix { get; set; }
        public double[,] R_Inv { get; set; }
        //This is the constructor, redefine the point?
        public GlobalCoordinateSystem(List<double> xyz, List<double> vector)
        {
            RefPnt = xyz;
            hyp = Math.Sqrt((vector[0] * vector[0] + vector[1] * vector[1]));
            Vector = vector;
            R = new double[,] { { vector[0] / hyp, -vector[1] / hyp, 0 }, { vector[1] / hyp, vector[0] / hyp, 0 }, { 0, 0, 1 } };
            R_Matrix = Matrix<double>.Build.DenseOfArray(R);
            R_Inv = R_Matrix.Inverse().ToArray();
        }
    }
}
