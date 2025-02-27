﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using ImageGlass.Base.DirectoryComparer;
using ImageGlass.Base.Photoing.Codecs;
using System.Globalization;

namespace ImageGlass.Base;

public partial class BHelper
{
    /// <summary>
    /// Convert string to float array, where numbers are separated by semicolons
    /// </summary>
    /// <param name="str">Input string. E.g. "12; -40; 50"</param>
    /// <param name="unsignedOnly">whether negative numbers are allowed</param>
    /// <param name="distinct">whether repitition of values is allowed</param>
    public static IEnumerable<float> StringToFloatArray(string str, bool unsignedOnly = false, bool distinct = false)
    {
        var numberStrings = str.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var numbers = new List<float>();

        foreach (var item in numberStrings)
        {
            // Issue #677: don't throw exception if we encounter invalid number,
            // e.g. the comma-separated zoom values from pre-V7.5
            if (!float.TryParse(item, NumberStyles.Float, Const.NumberFormat, out var num))
                continue;

            if (unsignedOnly && num < 0)
            {
                continue;
            }

            numbers.Add(num);
        }

        if (distinct)
        {
            return numbers.Distinct();
        }

        return numbers;
    }


    /// <summary>
    /// Center the given rectangle to the rectangle.
    /// </summary>
    /// <param name="rect1"></param>
    /// <param name="rect2"></param>
    /// <param name="limitRect1Size"></param>
    public static Rectangle CenterRectToRect(Rectangle rect1, Rectangle rect2, bool limitRect1Size = false)
    {
        var x = rect2.X + ((rect2.Width - rect1.Width) / 2);
        var y = rect2.Y + ((rect2.Height - rect1.Height) / 2);
        var width = rect1.Width;
        var height = rect1.Height;

        if (limitRect1Size)
        {
            x = Math.Max(rect2.X, x);
            y = Math.Max(rect2.Y, y);
            width = Math.Min(rect1.Width, rect2.Width);
            height = Math.Min(rect1.Height, rect2.Height);
        }

        return new Rectangle(x, y, width, height);
    }


    /// <summary>
    /// Get all controls by type
    /// </summary>
    public static IEnumerable<Control> GetAllControls(Control control, Type type)
    {
        var controls = control.Controls.Cast<Control>();

        return controls.SelectMany(ctrl => GetAllControls(ctrl, type))
                                  .Concat(controls)
                                  .Where(c => c.GetType() == type);
    }


    /// <summary>
    /// Checks if the given Windows version is matched.
    /// </summary>
    public static bool IsOS(WindowsOS ver)
    {
        if (ver == WindowsOS.Win11_22H2_OrLater)
        {
            return Environment.OSVersion.Version.Major >= 10
                && Environment.OSVersion.Version.Build >= 22621;
        }

        if (ver == WindowsOS.Win11OrLater)
        {
            return Environment.OSVersion.Version.Major >= 10
                && Environment.OSVersion.Version.Build >= 22000;
        }

        if (ver == WindowsOS.Win10)
        {
            return Environment.OSVersion.Version.Major == 10
                && Environment.OSVersion.Version.Build < 22000;
        }

        if (ver == WindowsOS.Win10OrLater)
        {
            return Environment.OSVersion.Version.Major >= 10;
        }


        return false;
    }


    /// <summary>
    /// Checks if the OS is Windows 10 or greater or equals the given build number.
    /// </summary>
    /// <param name="build">Build number of Windows.</param>
    public static bool IsOSBuildOrGreater(int build = -1)
    {
        return Environment.OSVersion.Version.Major >= 10
            && Environment.OSVersion.Version.Build >= build;
    }


    /// <summary>
    /// Sort image list.
    /// </summary>
    public static IEnumerable<string> SortImageList(IEnumerable<string> fileList,
        ImageOrderBy orderBy, ImageOrderType orderType, bool groupByDir)
    {
        // NOTE: relies on LocalSetting.ActiveImageLoadingOrder been updated first!

        // KBR 20190605
        // Fix observed limitation: to more closely match the Windows Explorer's sort
        // order, we must sort by the target column, then by name.
        var filePathComparer = new StringNaturalComparer(orderType == ImageOrderType.Asc, true);

        // initiate directory sorter to a comparer that does nothing
        // if user wants to group by directory, we initiate the real comparer
        var dirPathComparer = (IComparer<string?>)new IdentityComparer();
        if (groupByDir)
        {
            dirPathComparer = new StringNaturalComparer(orderType == ImageOrderType.Asc, true);
        }

        // KBR 20190605 Fix observed discrepancy: using UTC for create,
        // but not for write/access times

        // Sort image file
        if (orderBy == ImageOrderBy.FileSize)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => new FileInfo(f).Length)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => new FileInfo(f).Length)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by CreationTime
        if (orderBy == ImageOrderBy.DateCreated)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => new FileInfo(f).CreationTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => new FileInfo(f).CreationTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by Extension
        if (orderBy == ImageOrderBy.Extension)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => new FileInfo(f).Extension.ToLowerInvariant())
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => new FileInfo(f).Extension.ToLowerInvariant())
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by LastAccessTime
        if (orderBy == ImageOrderBy.DateAccessed)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => new FileInfo(f).LastAccessTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => new FileInfo(f).LastAccessTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by LastWriteTime
        if (orderBy == ImageOrderBy.DateModified)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => new FileInfo(f).LastWriteTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by Random
        if (orderBy == ImageOrderBy.Random)
        {
            // NOTE: ignoring the 'descending order' setting
            return fileList.AsParallel()
                .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                .ThenBy(_ => Guid.NewGuid());
        }

        // sort by Date
        if (orderBy == ImageOrderBy.Date)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => PhotoCodec.LoadMetadata(f).Date)
                    .ThenBy(f => Path.GetFileName(f), new StringNaturalComparer()); // always by ASC
            }
            else
            {
                return fileList
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => PhotoCodec.LoadMetadata(f).Date)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by DateTaken
        if (orderBy == ImageOrderBy.ExifDateTaken)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => PhotoCodec.LoadMetadata(f).ExifDateTimeOriginal)
                    .ThenBy(f => Path.GetFileName(f), new StringNaturalComparer()); // always by ASC
            }
            else
            {
                return fileList
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => PhotoCodec.LoadMetadata(f).ExifDateTimeOriginal)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by Rating
        if (orderBy == ImageOrderBy.ExifRating)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => PhotoCodec.LoadMetadata(f).ExifRatingPercent)
                    .ThenBy(f => Path.GetFileName(f), new StringNaturalComparer()); // always by ASC
            }
            else
            {
                return fileList
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => PhotoCodec.LoadMetadata(f).ExifRatingPercent)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }


        // sort by Name (default)
        return fileList.AsParallel()
            .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
            .ThenBy(f => Path.GetFileName(f), filePathComparer);
    }


    /// <summary>
    /// Gets selection rectangle from 2 points.
    /// </summary>
    /// <param name="point1">The first point</param>
    /// <param name="point2">The second point</param>
    /// <param name="aspectRatio">Aspect ratio</param>
    /// <param name="limitRect">The rectangle to limit the selection</param>
    public static RectangleF GetSelection(PointF? point1, PointF? point2,
        SizeF aspectRatio, float srcWidth, float srcHeight,
        RectangleF limitRect)
    {
        var selectedArea = new RectangleF();
        var fromPoint = point1 ?? new PointF();
        var toPoint = point2 ?? new PointF();

        if (fromPoint.IsEmpty || toPoint.IsEmpty) return selectedArea;

        // swap fromPoint and toPoint value if toPoint is less than fromPoint
        if (toPoint.X < fromPoint.X)
        {
            var tempX = fromPoint.X;
            fromPoint.X = toPoint.X;
            toPoint.X = tempX;
        }
        if (toPoint.Y < fromPoint.Y)
        {
            var tempY = fromPoint.Y;
            fromPoint.Y = toPoint.Y;
            toPoint.Y = tempY;
        }

        float width = Math.Abs(fromPoint.X - toPoint.X);
        float height = Math.Abs(fromPoint.Y - toPoint.Y);

        selectedArea.X = fromPoint.X;
        selectedArea.Y = fromPoint.Y;
        selectedArea.Width = width;
        selectedArea.Height = height;

        // limit the selected area to the limitRect
        selectedArea.Intersect(limitRect);


        // free aspect ratio
        if (aspectRatio.Width <= 0 || aspectRatio.Height <= 0)
            return selectedArea;


        var wRatio = aspectRatio.Width / aspectRatio.Height;
        var hRatio = aspectRatio.Height / aspectRatio.Width;

        // update selection size according to the ratio
        if (wRatio > hRatio)
        {
            selectedArea.Height = selectedArea.Width / wRatio;

            if (selectedArea.Bottom >= limitRect.Bottom)
            {
                var maxHeight = limitRect.Bottom - selectedArea.Y;
                selectedArea.Width = maxHeight * wRatio;
                selectedArea.Height = maxHeight;
            }
        }
        else
        {
            selectedArea.Width = selectedArea.Height / hRatio;

            if (selectedArea.Right >= limitRect.Right)
            {
                var maxWidth = limitRect.Right - selectedArea.X; ;
                selectedArea.Width = maxWidth;
                selectedArea.Height = maxWidth * hRatio;
            }
        }


        return selectedArea;
    }


    /// <summary>
    /// Opens ImageGlass site om Microsoft Store.
    /// </summary>
    public static void OpenImageGlassMsStore()
    {
        var campaignId = $"IgInAppBadgeV{App.Version}";
        var source = "AboutWindow";

        try
        {
            var url = $"ms-windows-store://pdp/?productid={Const.MS_APPSTORE_ID}&cid={campaignId}&referrer=appbadge&source={source}";

            _ = BHelper.OpenUrlAsync(url);
        }
        catch
        {
            try
            {
                var url = $"https://www.microsoft.com/store/productId/{Const.MS_APPSTORE_ID}?cid={campaignId}&referrer=appbadge&source={source}";

                _ = BHelper.OpenUrlAsync(url);
            }
            catch { }
        }
    }


}