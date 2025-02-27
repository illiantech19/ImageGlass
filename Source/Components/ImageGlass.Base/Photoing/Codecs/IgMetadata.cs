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
namespace ImageGlass.Base.Photoing.Codecs;


public class IgMetadata
{
    // File metadata
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;

    public DateTime FileCreationTime { get; set; } // local time
    public DateTime FileLastAccessTime { get; set; } // local time
    public DateTime FileLastWriteTime { get; set; } // local time
    public string FileCreationTimeFormated => BHelper.FormatDateTime(FileCreationTime);
    public string FileLastAccessTimeFormated => BHelper.FormatDateTime(FileLastAccessTime);
    public string FileLastWriteTimeFormated => BHelper.FormatDateTime(FileLastWriteTime);

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSize { get; set; } = 0;

    /// <summary>
    /// The formated file size. E.g. <c>32.09 MB</c>.
    /// </summary>
    public string FileSizeFormated => BHelper.FormatSize(FileSize);


    // Image data
    public int OriginalWidth { get; set; } = 0;
    public int OriginalHeight { get; set; } = 0;
    public int RenderedWidth { get; set; } = 0;
    public int RenderedHeight { get; set; } = 0;

    /// <summary>
    /// Gets the frame index of this metadata.
    /// </summary>
    public uint FrameIndex { get; set; } = 0;
    public int FrameCount { get; set; } = 0;
    public bool HasAlpha { get; set; } = false;
    public bool CanAnimate { get; set; } = false;

    public string ColorSpace { get; set; } = string.Empty;
    public string ColorProfile { get; set; } = string.Empty;


    // EXIF metadata
    public int ExifRatingPercent { get; set; } = 0;
    public DateTime? ExifDateTimeOriginal { get; set; } = null; // local time
    public DateTime? ExifDateTime { get; set; } = null; // local time
    public string? ExifImageDescription { get; set; } = null;
    public string? ExifModel { get; set; } = null;
    public string? ExifArtist { get; set; } = null;
    public string? ExifCopyright { get; set; } = null;
    public string? ExifSoftware { get; set; } = null;
    public float? ExifExposureTime { get; set; } = null;
    public float? ExifFNumber { get; set; } = null;
    public int? ExifISOSpeed { get; set; } = null;
    public float? ExifFocalLength { get; set; } = null;


    /// <summary>
    /// Auto-computes date.
    /// </summary>
    /// 
    /// <remarks>
    /// Returns <see cref="ExifDateTimeOriginal"/> if it's not <c>null</c>,
    /// else returns the earliest date of:
    /// <list type="bullet">
    ///     <item><see cref="ExifDateTime"/></item>,
    ///     <item><see cref="FileCreationTime"/></item>,
    ///     <item><see cref="FileLastWriteTime"/></item>,
    /// </list>
    /// </remarks>
    public DateTime Date
    {
        get
        {
            if (ExifDateTimeOriginal != null) return ExifDateTimeOriginal.Value;

            var dates = new List<DateTime?>()
            {
                ExifDateTime,
                FileCreationTime,
                FileLastWriteTime,
            };

            return dates.Where(i => i != null)
                .OrderBy(i => i)
                .FirstOrDefault() ?? DateTime.MaxValue;
        }
    }

}
