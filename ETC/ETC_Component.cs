/**
Copyright (c) 2019 hiron_rgrk

This software is released under the MIT License.
See LICENSE
**/

using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Types.Transforms;
using Rhino.Geometry;

namespace ETC{
    public class MakeEvolutionTower : GH_Component {
        /// <summary>
        /// エボリューションタワーの作成
        /// </summary>
        public MakeEvolutionTower()
            //     名称                  略称   ｺﾝﾎﾟｰﾈﾝﾄの説明          ｶﾃｺﾞﾘ   ｻﾌﾞｶﾃｺﾞﾘ
            : base("MakeEvolutionTower", "ETC", "Make Evolution Tower", "rgkr", "ETC") {
        }
        public override void ClearData() {
            base.ClearData();
        }
        /// <summary>
        /// インプットパラメータの登録
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddIntegerParameter("nFL", "nFL", "Number of Floor", GH_ParamAccess.item, 30);
            pManager.AddNumberParameter("Floor Height", "FH", "Floor Height", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("Angle", "Angle", "Rotation Angle", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("R", "R", "Columm Pipe Radius", GH_ParamAccess.item, 0.4);
            pManager.AddBrepParameter("Floor", "Floor", "Input Floor Brep", GH_ParamAccess.item);
        }
        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddBrepParameter("ET Floor", "ETFL", "Output Floor as Brep", GH_ParamAccess.item);
            pManager.AddBrepParameter("ET Columm", "ETCL", "Output Columm as Brep", GH_ParamAccess.list);
        }
        /// <summary>
        /// 計算部分
        /// </summary>
        /// <param name="DA">インプットパラメータからデータを取得し、出力用のデータを保持するオブジェクト</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            int nFL = 0;
            double FH = 0, Angle = 0, R = 0;
            IGH_GeometricGoo FL = null;
            Brep Floor = null;

            // 入力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetData(0, ref nFL)) { return; }
            if (!DA.GetData(1, ref FH)) { return; }
            if (!DA.GetData(2, ref Angle)) { return; }
            if (!DA.GetData(3, ref R)) { return; }
            if (!DA.GetData(4, ref FL)) { return; }
            if (!DA.GetData(4, ref Floor)) { return; }

            // 床の作成
            List<IGH_GeometricGoo> list = new List<IGH_GeometricGoo>(nFL);
            Vector3d DirectionVector = new Vector3d(0, 0, FH);
            Point3d Center = new Point3d(0, 0, 0);
            for (int i = 0; i < nFL; i++) {
                ITransform transform  = new Translation(DirectionVector * (double)i);
                ITransform transform2 = new Rotation(Center, DirectionVector, Angle * (double)i);
                if (FL != null) {
                    IGH_GeometricGoo FL2 = FL.DuplicateGeometry();
                    FL2 = FL2.Transform(transform.ToMatrix());
                    FL2 = FL2.Transform(transform2.ToMatrix());
                    list.Add(FL2);
                }
                else {
                    list.Add(null);
                }
            }

            // 柱の作成
            List<Brep> collist = new List<Brep>(nFL);
            Point3d[] CornerPoints = Floor.DuplicateVertices();
            // 内柱
            Point3d Point1in = new Point3d(CornerPoints[0].X / 2.0, CornerPoints[0].Y / 2.0, CornerPoints[0].Z / 2.0);
            Point3d Point2in = new Point3d(CornerPoints[1].X / 2.0, CornerPoints[1].Y / 2.0, CornerPoints[1].Z / 2.0);
            Point3d Point3in = new Point3d(CornerPoints[2].X / 2.0, CornerPoints[2].Y / 2.0, CornerPoints[2].Z / 2.0);
            Point3d Point4in = new Point3d(CornerPoints[3].X / 2.0, CornerPoints[3].Y / 2.0, CornerPoints[3].Z / 2.0);
            // 外柱
            Point3d Point1outS = new Point3d(CornerPoints[0].X, CornerPoints[0].Y, CornerPoints[0].Z);
            Point3d Point2outS = new Point3d(CornerPoints[1].X, CornerPoints[1].Y, CornerPoints[1].Z);
            Point3d Point3outS = new Point3d(CornerPoints[2].X, CornerPoints[2].Y, CornerPoints[2].Z);
            Point3d Point4outS = new Point3d(CornerPoints[3].X, CornerPoints[3].Y, CornerPoints[3].Z);
            Point3d Point1outE = Point1outS;
            Point3d Point2outE = Point2outS;
            Point3d Point3outE = Point3outS;
            Point3d Point4outE = Point4outS;

            double num1 = GH_Component.DocumentTolerance();
            double num2 = GH_Component.DocumentAngleTolerance();
            for (int i = 0; i < nFL-1; i++) {
                if (FL != null) {
                    if (i != 0) {
                        // 内柱
                        Point1in = new Point3d(Point1in.X, Point1in.Y, Point1in.Z + FH);
                        Point2in = new Point3d(Point2in.X, Point2in.Y, Point2in.Z + FH);
                        Point3in = new Point3d(Point3in.X, Point3in.Y, Point3in.Z + FH);
                        Point4in = new Point3d(Point4in.X, Point4in.Y, Point4in.Z + FH);
                        // 外柱
                        Point1outS = new Point3d(Point1outE.X, Point1outE.Y, Point1outE.Z);
                        Point2outS = new Point3d(Point2outE.X, Point2outE.Y, Point2outE.Z);
                        Point3outS = new Point3d(Point3outE.X, Point3outE.Y, Point3outE.Z);
                        Point4outS = new Point3d(Point4outE.X, Point4outE.Y, Point4outE.Z);
                    }

                    // 外柱 終点
                    Point1outE = new Point3d(
                        Point1outS.X * Math.Cos(Angle) - Point1outS.Y * Math.Sin(Angle),
                        Point1outS.X * Math.Sin(Angle) + Point1outS.Y * Math.Cos(Angle),
                        Point1outS.Z + FH);
                    Point2outE = new Point3d(
                        Point2outS.X * Math.Cos(Angle) - Point2outS.Y * Math.Sin(Angle),
                        Point2outS.X * Math.Sin(Angle) + Point2outS.Y * Math.Cos(Angle),
                        Point2outS.Z + FH);
                    Point3outE = new Point3d(
                        Point3outS.X * Math.Cos(Angle) - Point3outS.Y * Math.Sin(Angle),
                        Point3outS.X * Math.Sin(Angle) + Point3outS.Y * Math.Cos(Angle),
                        Point3outS.Z + FH);
                    Point4outE = new Point3d(
                        Point4outS.X * Math.Cos(Angle) - Point4outS.Y * Math.Sin(Angle),
                        Point4outS.X * Math.Sin(Angle) + Point4outS.Y * Math.Cos(Angle),
                        Point4outS.Z + FH);

                    LineCurve LineCurve1in = new LineCurve(new Line(Point1in, DirectionVector));
                    LineCurve LineCurve2in = new LineCurve(new Line(Point2in, DirectionVector));
                    LineCurve LineCurve3in = new LineCurve(new Line(Point3in, DirectionVector));
                    LineCurve LineCurve4in = new LineCurve(new Line(Point4in, DirectionVector));
                    //
                    LineCurve LineCurve1out = new LineCurve(new Line(Point1outS, Point1outE));
                    LineCurve LineCurve2out = new LineCurve(new Line(Point2outS, Point2outE));
                    LineCurve LineCurve3out = new LineCurve(new Line(Point3outS, Point3outE));
                    LineCurve LineCurve4out = new LineCurve(new Line(Point4outS, Point4outE));

                    Brep[] col1in = Brep.CreatePipe(LineCurve1in, R, true, 0, false, num1, num2);
                    Brep[] col2in = Brep.CreatePipe(LineCurve2in, R, true, 0, false, num1, num2);
                    Brep[] col3in = Brep.CreatePipe(LineCurve3in, R, true, 0, false, num1, num2);
                    Brep[] col4in = Brep.CreatePipe(LineCurve4in, R, true, 0, false, num1, num2);
                    //
                    Brep[] col1out = Brep.CreatePipe(LineCurve1out, R, true, 0, false, num1, num2);
                    Brep[] col2out = Brep.CreatePipe(LineCurve2out, R, true, 0, false, num1, num2);
                    Brep[] col3out = Brep.CreatePipe(LineCurve3out, R, true, 0, false, num1, num2);
                    Brep[] col4out = Brep.CreatePipe(LineCurve4out, R, true, 0, false, num1, num2);

                    collist.AddRange(col1in);
                    collist.AddRange(col2in);
                    collist.AddRange(col3in);
                    collist.AddRange(col4in);
                    collist.AddRange(col1out);
                    collist.AddRange(col2out);
                    collist.AddRange(col3out);
                    collist.AddRange(col4out);
                }
            }
            // 出力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetDataList(0, list);
            DA.SetDataList(1, collist);

        }
        /// <summary>
        /// アイコンの設定。24x24 pixelsが推奨
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return ETC.Properties.Resource.ETCicon;
            }
        }
        /// <summary>
        /// GUIDの設定
        /// </summary>
        public override Guid ComponentGuid {
            get {
                return new Guid("621eac11-23fb-445c-9430-44ce37bf9040");
            }
        }
    }
}
