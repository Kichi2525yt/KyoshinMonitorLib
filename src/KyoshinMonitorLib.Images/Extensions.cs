﻿using KyoshinMonitorLib.UrlGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KyoshinMonitorLib.Images
{
	public static class Extensions
	{
		/// <summary>
		/// 与えられた情報から強震モニタの画像を取得し、そこから観測点情報を使用し震度を取得します。
		/// <para>asyncなのはStream取得部分のみなので注意してください。</para>
		/// </summary>
		/// <param name="points">使用する観測点情報の配列</param>
		/// <param name="datetime">参照する日付</param>
		/// <param name="isBehole">地中の情報を取得するかどうか</param>
		/// <returns>震度情報が追加された観測点情報の配列</returns>
		public static async Task<IEnumerable<ImageAnalysisResult>> ParseIntensityFromParameterAsync(this WebApi webApi, IEnumerable<ObservationPoint> points, DateTime datetime, bool isBehole = false)
			=> await webApi.ParseIntensityFromParameterAsync(points.Select(p => new ImageAnalysisResult(p)).ToArray(), datetime, isBehole);

		/// <summary>
		/// 与えられた情報から強震モニタの画像を取得し、そこから観測点情報を使用し震度を取得します。
		/// <para>asyncなのはStream取得部分のみなので注意してください。</para>
		/// </summary>
		/// <param name="points">使用する観測点情報の配列</param>
		/// <param name="datetime">参照する日付</param>
		/// <param name="isBehole">地中の情報を取得するかどうか</param>
		/// <returns>震度情報が追加された観測点情報の配列</returns>
		public static async Task<IEnumerable<ImageAnalysisResult>> ParseIntensityFromParameterAsync(this WebApi webApi, IEnumerable<ImageAnalysisResult> points, DateTime datetime, bool isBehole = false)
		{
			using (var stream = new MemoryStream(await webApi.GetRealtimeImageData(datetime, RealTimeDataType.Shindo, isBehole)))
			using (var bitmap = new Bitmap(stream))
				return points.ParseIntensityFromImage(bitmap);
		}

		/// <summary>
		/// 与えられた画像から観測点情報を使用し震度を取得します。
		/// </summary>
		/// <param name="points">使用する観測点情報の配列</param>
		/// <param name="bitmap">参照する画像</param>
		/// <returns>震度情報が追加された観測点情報の配列</returns>
		public static IEnumerable<ImageAnalysisResult> ParseIntensityFromImage(this IEnumerable<ImageAnalysisResult> points, Bitmap bitmap)
		{
			foreach (var point in points)
			{
				if (point.ObservationPoint.Point == null || point.ObservationPoint.IsSuspended)
				{
					point.AnalysisResult = null;
					continue;
				}

				try
				{
					var color = bitmap.GetPixel(point.ObservationPoint.Point.Value.X, point.ObservationPoint.Point.Value.Y);
					point.Color = color;
					point.AnalysisResult = ColorToIntensityConverter.Convert(color);
				}
				catch (Exception ex)
				{
					point.AnalysisResult = null;
					Debug.WriteLine("parseEx: " + ex.ToString());
				}
			}
			return points;
		}
	}
}
