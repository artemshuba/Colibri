using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Colibri.Helpers
{
    //TODO
    public static class PhotoSpacier
    {
        public static void Calculate(Grid container)
        {
            if (((ICollection<UIElement>)container.Children).Count == 0)
                return;
            List<Size> childrenRects = new List<Size>();
            foreach (FrameworkElement frameworkElement1 in (IEnumerable<UIElement>)container.Children)
            {
                double width;
                double height;
                if (!((IDictionary<object, object>)frameworkElement1.Resources).ContainsKey((object)"PhotosContainerWidth"))
                {
                    width = double.IsInfinity(frameworkElement1.MaxWidth) ? 100.0 : frameworkElement1.MaxWidth;
                    height = double.IsInfinity(frameworkElement1.MaxHeight) ? 100.0 : frameworkElement1.MaxHeight;
                    ((IDictionary<object, object>)frameworkElement1.Resources).Add((object)"PhotosContainerWidth", (object)width);
                    ((IDictionary<object, object>)frameworkElement1.Resources).Add((object)"PhotosContainerHeight", (object)height);
                    FrameworkElement frameworkElement2 = frameworkElement1;
                    double num1;
                    double num2 = num1 = double.MaxValue;
                    frameworkElement2.MaxHeight = num1;
                    double num3 = num2;
                    frameworkElement2.MaxWidth = num3;
                }
                else
                {
                    width = (double)((IDictionary<object, object>)frameworkElement1.Resources)[(object)"PhotosContainerWidth"];
                    height = (double)((IDictionary<object, object>)frameworkElement1.Resources)[(object)"PhotosContainerHeight"];
                }
                childrenRects.Add(new Size(width, height));
            }
            List<Rect> list;
            if (childrenRects.Count > 1)
            {
                list = PhotoSpacier.CreateLayout(new Size(container.MaxWidth, container.MaxHeight), childrenRects, 5.0);
            }
            else
            {
                Size size = childrenRects[0];
                double width1 = size.Width;
                size = childrenRects[0];
                double height1 = size.Height;
                //if (height1 > container.MaxHeight)
                //{
                //    height1 = container.MaxHeight;
                //    double num = width1 * height1;
                //    size = childrenRects[0];
                //    double height2 = size.Height;
                //    width1 = num / height2;
                //}
                //if (width1 > container.MaxWidth)
                //{
                //    width1 = container.MaxWidth;
                //    double num = height1 * width1;
                //    size = childrenRects[0];
                //    double width2 = size.Width;
                //    height1 = num / width2;
                //}

                if (width1 > container.MaxWidth || height1 > container.MaxHeight)
                {

                    var ratioX = (double)container.MaxWidth / width1;
                    var ratioY = (double)container.MaxHeight / height1;
                    var ratio = Math.Min(ratioX, ratioY);

                    width1 = (int)(width1 * ratio);
                    height1 = (int)(height1 * ratio);
                }

                list = new List<Rect>()
        {
          new Rect(0.0, 0.0, width1, height1)
        };
            }
            double num4 = Enumerable.Min(Enumerable.Concat<double>(Enumerable.Select<Rect, double>((IEnumerable<Rect>)list, (Func<Rect, double>)(v => v.X)), (IEnumerable<double>)new double[1]
            {
        double.MaxValue
            }));
            if (num4 > 0.0)
            {
                for (int index = 0; index < list.Count; ++index)
                {
                    Rect rect = list[index];
                    list[index] = new Rect(rect.X - num4, rect.Y, rect.Width, rect.Height);
                }
            }
            double num5 = Enumerable.Max(Enumerable.Concat<double>(Enumerable.Select<Rect, double>((IEnumerable<Rect>)list, (Func<Rect, double>)(s => s.Top + s.Height)), (IEnumerable<double>)new double[1]));
            if (num5 > container.MaxHeight)
            {
                double num1 = container.MaxHeight / num5;
                for (int index = 0; index < list.Count; ++index)
                {
                    Rect rect = list[index];
                    list[index] = new Rect(rect.X * num1, rect.Y * num1, rect.Width * num1, rect.Height * num1);
                }
            }
            for (int index = 0; index < list.Count; ++index)
            {
                FrameworkElement frameworkElement = (FrameworkElement)((IList<UIElement>)container.Children)[index];
                Rect rect = list[index];
                double num1 = Math.Round(rect.Height);
                frameworkElement.Height = num1;
                double num2 = Math.Round(rect.Width);
                frameworkElement.Width = num2;
                Thickness thickness = new Thickness(Math.Round(rect.X), Math.Round(rect.Y), 0.0, 0.0);
                frameworkElement.Margin = thickness;
                int num3 = 0;
                frameworkElement.HorizontalAlignment = (HorizontalAlignment)num3;
                int num6 = 0;
                frameworkElement.VerticalAlignment = (VerticalAlignment)num6;
            }
        }

        public static List<Rect> CreateLayout(Size parentRect, List<Size> childrenRects, double marginBetween)
        {
            List<PhotoSpacier.ThumbAttachment> list1 = Enumerable.ToList<PhotoSpacier.ThumbAttachment>(Enumerable.Select<Size, PhotoSpacier.ThumbAttachment>((IEnumerable<Size>)childrenRects, (Func<Size, PhotoSpacier.ThumbAttachment>)(r => new PhotoSpacier.ThumbAttachment()
            {
                Height = r.Height > 0.0 ? r.Height : 100.0,
                Width = r.Width > 0.0 ? r.Width : 100.0
            })));
            string str = "";
            int[] numArray = new int[3];
            List<double> list2 = new List<double>();
            int count = list1.Count;
            bool flag = false;
            foreach (double num in Enumerable.Select<PhotoSpacier.ThumbAttachment, double>((IEnumerable<PhotoSpacier.ThumbAttachment>)list1, (Func<PhotoSpacier.ThumbAttachment, double>)(thumb => thumb.getRatio())))
            {
                if (num == -1.0)
                    flag = true;
                char ch = num > 1.2 ? 'w' : (num < 0.8 ? 'n' : 'q');
                str += ch.ToString();
                int index = 0;
                if ((int)ch != 110)
                {
                    if ((int)ch != 113)
                    {
                        if ((int)ch == 119)
                            index = 0;
                    }
                    else
                        index = 2;
                }
                else
                    index = 1;
                ++numArray[index];
                list2.Add(num);
            }
            if (!flag)
            {
                double num1 = list2.Count > 0 ? Enumerable.Sum((IEnumerable<double>)list2) / (double)list2.Count : 1.0;
                double margin = marginBetween;
                double num2 = marginBetween;
                double width1 = parentRect.Width;
                double height1 = parentRect.Height;
                double num3 = width1 / height1;
                switch (count)
                {
                    case 1:
                        if (list2[0] > 0.8)
                        {
                            PhotoSpacier.ThumbAttachment thumbAttachment = list1[0];
                            double width2 = width1;
                            double num4 = list2[0];
                            double height2 = width2 / num4;
                            int num5 = 0;
                            int num6 = 0;
                            thumbAttachment.SetViewSize(width2, height2, num5 != 0, num6 != 0);
                            break;
                        }
                        list1[0].SetViewSize(height1 * list2[0], height1, false, false);
                        break;
                    case 2:
                        if (str == "ww" && num1 > 1.4 * num3 && list2[1] - list2[0] < 0.2)
                        {
                            double width2 = width1;
                            double height2 = Math.Min(width2 / list2[0], Math.Min(width2 / list2[1], (height1 - num2) / 2.0));
                            list1[0].SetViewSize(width2, height2, true, false);
                            list1[1].SetViewSize(width2, height2, false, false);
                            break;
                        }
                        if (str == "ww" || str == "qq")
                        {
                            double width2 = (width1 - margin) / 2.0;
                            double height2 = Math.Min(width2 / list2[0], Math.Min(width2 / list2[1], height1));
                            list1[0].SetViewSize(width2, height2, false, false);
                            list1[1].SetViewSize(width2, height2, false, false);
                            break;
                        }
                        double width3 = (width1 - margin) / list2[1] / (1.0 / list2[0] + 1.0 / list2[1]);
                        double width4 = width1 - width3 - margin;
                        double height3 = Math.Min(height1, Math.Min(width3 / list2[0], width4 / list2[1]));
                        list1[0].SetViewSize(width3, height3, false, false);
                        list1[1].SetViewSize(width4, height3, false, false);
                        break;
                    case 3:
                        if (str == "www")
                        {
                            double width2 = width1;
                            double height2 = Math.Min(width2 / list2[0], (height1 - num2) * 0.66);
                            list1[0].SetViewSize(width2, height2, true, false);
                            double width5 = (width1 - margin) / 2.0;
                            double height4 = Math.Min(height1 - height2 - num2, Math.Min(width5 / list2[1], width5 / list2[2]));
                            list1[1].SetViewSize(width5, height4, false, false);
                            list1[2].SetViewSize(width5, height4, false, false);
                            break;
                        }
                        double height5 = height1;
                        double width6 = Math.Min(height5 * list2[0], (width1 - margin) * 0.75);
                        list1[0].SetViewSize(width6, height5, false, false);
                        double height6 = list2[1] * (height1 - num2) / (list2[2] + list2[1]);
                        double height7 = height1 - height6 - num2;
                        double width7 = Math.Min(width1 - width6 - margin, Math.Min(height6 * list2[2], height7 * list2[1]));
                        list1[1].SetViewSize(width7, height7, false, true);
                        list1[2].SetViewSize(width7, height6, false, true);
                        break;
                    case 4:
                        if (str == "wwww")
                        {
                            double width2 = width1;
                            double height2 = Math.Min(width2 / list2[0], (height1 - num2) * 0.66);
                            list1[0].SetViewSize(width2, height2, true, false);
                            double val2 = (width1 - 2.0 * margin) / (list2[1] + list2[2] + list2[3]);
                            double width5 = val2 * list2[1];
                            double width8 = val2 * list2[2];
                            double width9 = val2 * list2[3];
                            double height4 = Math.Min(height1 - height2 - num2, val2);
                            list1[1].SetViewSize(width5, height4, false, false);
                            list1[2].SetViewSize(width8, height4, false, false);
                            list1[3].SetViewSize(width9, height4, false, false);
                            break;
                        }
                        double height8 = height1;
                        double width10 = Math.Min(height8 * list2[0], (width1 - margin) * 0.66);
                        list1[0].SetViewSize(width10, height8, false, false);
                        double val2_1 = (height1 - 2.0 * num2) / (1.0 / list2[1] + 1.0 / list2[2] + 1.0 / list2[3]);
                        double height9 = val2_1 / list2[1];
                        double height10 = val2_1 / list2[2];
                        double height11 = val2_1 / list2[3];
                        double width11 = Math.Min(width1 - width10 - margin, val2_1);
                        list1[1].SetViewSize(width11, height9, false, true);
                        list1[2].SetViewSize(width11, height10, false, true);
                        list1[3].SetViewSize(width11, height11, false, true);
                        break;
                    default:
                        List<double> list3 = new List<double>();
                        list3.AddRange(num1 > 1.1 ? Enumerable.Select<double, double>((IEnumerable<double>)list2, (Func<double, double>)(ratio => Math.Max(1.0, ratio))) : Enumerable.Select<double, double>((IEnumerable<double>)list2, (Func<double, double>)(ratio => Math.Min(1.0, ratio))));
                        Dictionary<string, List<double>> dictionary1 = new Dictionary<string, List<double>>();
                        dictionary1[string.Concat((object)count)] = new List<double>()
            {
              PhotoSpacier.calculateMultiThumbsHeight(list3, width1, margin)
            };
                        for (int index = 1; index <= count - 1; ++index)
                            dictionary1[index.ToString() + "," + (count - index).ToString()] = new List<double>()
              {
                PhotoSpacier.calculateMultiThumbsHeight(PhotoSpacier.Sublist<double>(list3, 0, index), width1, margin),
                PhotoSpacier.calculateMultiThumbsHeight(PhotoSpacier.Sublist<double>(list3, index, list3.Count), width1, margin)
              };
                        for (int end1 = 1; end1 <= count - 2; ++end1)
                        {
                            for (int index1 = 1; index1 <= count - end1 - 1; ++index1)
                            {
                                Dictionary<string, List<double>> dictionary2 = dictionary1;
                                string index2 = end1 + "," + index1 + "," + (count - end1 - index1);
                                List<double> list4 = new List<double>();
                                list4.Add(PhotoSpacier.calculateMultiThumbsHeight(PhotoSpacier.Sublist<double>(list3, 0, end1), width1, margin));
                                List<double> list5 = list3;
                                int begin = end1;
                                int num4 = index1;
                                int end2 = begin + num4;
                                double num5 = PhotoSpacier.calculateMultiThumbsHeight(PhotoSpacier.Sublist<double>(list5, begin, end2), width1, margin);
                                list4.Add(num5);
                                double num6 = PhotoSpacier.calculateMultiThumbsHeight(PhotoSpacier.Sublist<double>(list3, end1 + index1, list3.Count), width1, margin);
                                list4.Add(num6);
                                dictionary2[index2] = list4;
                            }
                        }
                        string index3 = (string)null;
                        double num7 = 0.0;
                        foreach (string index1 in dictionary1.Keys)
                        {
                            List<double> list4 = dictionary1[index1];
                            double num4 = Math.Abs(num2 * (double)(list4.Count - 1) + Enumerable.Sum((IEnumerable<double>)list4) - height1);
                            if (index1.IndexOf(",") != -1)
                            {
                                string[] strArray = index1.Split(',');
                                if (int.Parse(strArray[0]) > int.Parse(strArray[1]) || strArray.Length > 2 && int.Parse(strArray[1]) > int.Parse(strArray[2]))
                                    num4 *= 1.1;
                            }
                            if (index3 == null || num4 < num7)
                            {
                                index3 = index1;
                                num7 = num4;
                            }
                        }
                        List<PhotoSpacier.ThumbAttachment> list6 = new List<PhotoSpacier.ThumbAttachment>((IEnumerable<PhotoSpacier.ThumbAttachment>)list1);
                        List<double> list7 = new List<double>((IEnumerable<double>)list3);
                        string[] strArray1 = index3.Split(',');
                        List<double> list8 = dictionary1[index3];
                        int index4 = 0;
                        foreach (string s in strArray1)
                        {
                            int num4 = int.Parse(s);
                            List<PhotoSpacier.ThumbAttachment> list4 = new List<PhotoSpacier.ThumbAttachment>();
                            for (int index1 = 0; index1 < num4; ++index1)
                            {
                                list4.Add(list6[0]);
                                list6.RemoveAt(0);
                            }
                            double num5 = list8[index4];
                            ++index4;
                            int num6 = list4.Count - 1;
                            for (int index1 = 0; index1 < list4.Count; ++index1)
                            {
                                PhotoSpacier.ThumbAttachment thumbAttachment = list4[index1];
                                double num8 = list7[0];
                                list7.RemoveAt(0);
                                double width2 = num8 * num5;
                                double height2 = num5;
                                int num9 = index1 == num6 ? 1 : 0;
                                int num10 = 0;
                                thumbAttachment.SetViewSize(width2, height2, num9 != 0, num10 != 0);
                            }
                        }
                        break;
                }
            }
            List<Rect> list9 = new List<Rect>(list1.Count);
            double y = 0.0;
            double num11 = 0.0;
            foreach (PhotoSpacier.ThumbAttachment thumbAttachment in list1)
            {
                num11 += thumbAttachment.CalcWidth;
                num11 += marginBetween;
                if (!thumbAttachment.LastRow)
                {
                    if (thumbAttachment.LastColumn)
                        break;
                }
                else
                    break;
            }
            if (num11 > 0.0)
                num11 -= marginBetween;
            double num12 = parentRect.Width / 2.0 - num11 / 2.0;
            double x = num12;
            foreach (PhotoSpacier.ThumbAttachment thumbAttachment in list1)
            {
                list9.Add(new Rect(x, y, thumbAttachment.CalcWidth, thumbAttachment.CalcHeight));
                if (!thumbAttachment.LastColumn && !thumbAttachment.LastRow)
                    x += thumbAttachment.CalcWidth + marginBetween;
                else if (thumbAttachment.LastRow)
                    y += thumbAttachment.CalcHeight + marginBetween;
                else if (thumbAttachment.LastColumn)
                {
                    x = num12;
                    y += thumbAttachment.CalcHeight + marginBetween;
                }
            }
            return list9;
        }

        private static double calculateMultiThumbsHeight(List<double> ratios, double width, double margin)
        {
            return (width - (double)(ratios.Count - 1) * margin) / Enumerable.Sum((IEnumerable<double>)ratios);
        }

        public static List<T> Sublist<T>(List<T> list, int begin, int end)
        {
            List<T> list1 = new List<T>();
            for (int index = begin; index < end; ++index)
                list1.Add(list[index]);
            return list1;
        }

        public sealed class ThumbAttachment
        {
            public double Width { get; set; }

            public double Height { get; set; }

            public double CalcWidth { get; set; }

            public double CalcHeight { get; set; }

            public bool LastColumn { get; set; }

            public bool LastRow { get; set; }

            public double getRatio()
            {
                return this.Width / this.Height;
            }

            public void SetViewSize(double width, double height, bool lastColumn, bool lastRow)
            {
                this.CalcWidth = width;
                this.CalcHeight = height;
                this.LastColumn = lastColumn;
                this.LastRow = lastRow;
            }
        }
    }

    //public static class PhotosContainer
    //{
    //    public static void Calculate(Grid container)
    //    {
    //        if (container.Children.Count == 0)
    //            return;

    //        var childrenRects = new List<Size>();

    //        foreach (FrameworkElement child in container.Children)
    //        {
    //            double width;
    //            double height;
    //            if (!child.Resources.ContainsKey("PhotosContainerWidth"))
    //            {
    //                width = double.IsInfinity(child.MaxWidth) ? 100.0d : child.MaxWidth;
    //                height = double.IsInfinity(child.MaxHeight) ? 100.0d : child.MaxHeight;

    //                child.Resources.Add("PhotosContainerWidth", width);
    //                child.Resources.Add("PhotosContainerHeight", height);

    //                child.MaxHeight = double.MaxValue;
    //                child.MaxHeight = double.MaxValue;
    //            }
    //            else
    //            {
    //                width = (double)child.Resources["PhotosContainerWidth"];
    //                height = (double)child.Resources["PhotosContainerHeight"];
    //            }

    //            childrenRects.Add(new Size(width, height));
    //        }

    //        var rects = new List<Rect>();
    //    }

    //    private static List<Rect> CreateLayout(Size parentRect, List<Size> childrenRects, double marginBetween)
    //    {
    //        var thumbs = childrenRects.Select(r => new ThumbAttachment()
    //        {
    //            Height = r.Height > 0 ? r.Height : 100,
    //            Width = r.Width > 0 ? r.Width : 100
    //        }).ToList();

    //        string str = "";
    //        int[] numArray = new int[3];

    //        List<double> ratios = new List<double>();

    //        bool flag = false;
    //        foreach (var ratio in thumbs.Select(a => a.GetRatio()))
    //        {
    //            if (ratio == -1.0)
    //                flag = true;

    //            var ch = ratio > 1.2 ? 'w' : (ratio < 0.8 ? 'n' : 'q');
    //            str += ch;

    //            int index = 0;
    //            if ((int)ch != 110)
    //            {
    //                if ((int)ch != 113)
    //                {
    //                    if ((int)ch == 119)
    //                        index = 0;
    //                }
    //                else
    //                    index = 2;
    //            }
    //            else
    //                index = 1;

    //            ++numArray[index];
    //            ratios.Add(ratio);
    //        }

    //        if (!flag)
    //        {
    //            var num1 = ratios.Count > 0 ? ratios.Sum() / ratios.Count : 1;
    //            var margin = marginBetween;
    //            var width1 = parentRect.Width;
    //            var height1 = parentRect.Height;
    //            var num3 = width1 / height1;

    //            switch (thumbs.Count)
    //            {
    //                case 1:
    //                    if (ratios[0] > 0.8)
    //                    {
    //                        thumbs[0].SetViewSize(width1, width1 / ratios[0], false, false);
    //                    }
    //                    else
    //                        thumbs[0].SetViewSize(height1 * ratios[0], height1, false, false);
    //                    break;

    //                case 2:
    //                    if (str == "ww" && num1 > 1.4 * num3 && ratios[1] - ratios[0] < 0.2)
    //                    {
    //                        var height2 = Math.Min(width1 / ratios[0], Math.Min(width1 / ratios[1], (height1 - margin) / 2));
    //                        thumbs[0].SetViewSize(width1, height2, true, false);
    //                        thumbs[1].SetViewSize(width1, height2, false, false);
    //                    }
    //                    else if (str == "ww" || str == "qq")
    //                    {
    //                        var width2 = (width1 - margin) / 2;
    //                        var height2 = Math.Min(width2 / ratios[0], Math.Min(width2 / ratios[1], height1));
    //                        thumbs[0].SetViewSize(width2, height2, false, false);
    //                        thumbs[1].SetViewSize(width2, height2, false, false);
    //                    }
    //                    break;
    //            }
    //        }
    //    }

    //    private class ThumbAttachment
    //    {
    //        public double Width { get; set; }

    //        public double Height { get; set; }

    //        public double CalcWidth { get; set; }

    //        public double CalcHeight { get; set; }

    //        public bool LastColumn { get; set; }

    //        public bool LastRow { get; set; }

    //        public double GetRatio()
    //        {
    //            return this.Width / this.Height;
    //        }

    //        public void SetViewSize(double width, double height, bool lastColumn, bool lastRow)
    //        {
    //            this.CalcWidth = width;
    //            this.CalcHeight = height;
    //            this.LastColumn = lastColumn;
    //            this.LastRow = lastRow;
    //        }
    //    }
    //}
}
