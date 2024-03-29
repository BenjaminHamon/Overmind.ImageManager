﻿using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Overmind.ImageManager.Model.Downloads
{
	public class Downloader : IDownloader
	{
		private const int BufferSize = 1024 * 1024;

		public Downloader(HttpClient httpClient)
		{
			this.httpClient = httpClient;
		}

		private readonly HttpClient httpClient;

		public async Task<byte[]> Download(Uri uri, ProgressHandler progressHandler, CancellationToken cancellationToken)
		{
			if (uri.IsFile)
				return await DownloadWithFile(uri, progressHandler, cancellationToken);

			if ((uri.Scheme == "http") || (uri.Scheme == "https"))
				return await DownloadWithHttp(uri, progressHandler, cancellationToken);

			throw new ArgumentException(String.Format("Scheme is unsupported: '{0}'", uri.Scheme));
		}

		public async Task<byte[]> DownloadWithFile(Uri uri, ProgressHandler progressHandler, CancellationToken cancellationToken)
		{
			using (FileStream fileStream = File.OpenRead(uri.LocalPath))
				return await DownloadWithStream(uri, fileStream, fileStream.Length, progressHandler, cancellationToken);
		}

		public async Task<byte[]> DownloadWithHttp(Uri uri, ProgressHandler progressHandler, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			using (HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
			{
				response.EnsureSuccessStatusCode();

				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
					return await DownloadWithStream(uri, responseStream, response.Content.Headers.ContentLength, progressHandler, cancellationToken);
			}
		}

		public async Task<byte[]> DownloadWithStream(Uri uri, Stream stream, long? totalSize, ProgressHandler progressHandler, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			using (MemoryStream dataStream = new MemoryStream())
			{
				byte[] buffer = new byte[BufferSize];
				Stopwatch timer = Stopwatch.StartNew();

				while (true)
				{
					int length = await stream.ReadAsync(buffer, 0, buffer.Length);
					dataStream.Write(buffer, 0, length);

					if ((length == 0) || (timer.ElapsedMilliseconds > 10))
					{
						progressHandler?.Invoke(uri, dataStream.Length, totalSize ?? 0);
						timer.Restart();
					}

					cancellationToken.ThrowIfCancellationRequested();

					if (length == 0)
						break;
				}

				timer.Stop();

				return dataStream.ToArray();
			}
		}

		public async Task<Uri> ResolveUri(Uri uri, string expression, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			List<string> commandList = expression.Split(new string[] { "|" }, StringSplitOptions.None)
				.Select(command => command.Trim()).ToList();

			string result = uri.ToString();

			foreach (string command in commandList)
			{
				List<string> commandElements = command.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

				switch (commandElements[0])
				{
					case "Request": result = await ExecuteRequest(String.Format(commandElements.ElementAtOrDefault(1) ?? result, uri.Segments), cancellationToken); break;
					case "Json": result = ExecuteJson(result, String.Format(commandElements[1], uri.Segments)); break;
					case "XPath": result = ExecuteXPath(result, String.Format(commandElements[1], uri.Segments)); break;
					default: throw new ArgumentException(String.Format("Unknown operation '{0}'", commandElements[0]));
				}
			}

			return new Uri(result);
		}

		private async Task<string> ExecuteRequest(string uri, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			using (HttpResponseMessage response = await httpClient.GetAsync(uri, cancellationToken))
			{
				response.EnsureSuccessStatusCode();

				return await response.Content.ReadAsStringAsync();
			}
		}

		private string ExecuteJson(string json, string path)
		{
			JToken jsonToken = JObject.Parse(json).SelectToken(path);
			if (jsonToken == null)
				throw new InvalidDataException("The JSON path matched no value");

			return jsonToken.Value<string>();
		}

		private string ExecuteXPath(string data, string xPath)
		{
			HtmlDocument htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(data);

			XPathNavigator xPathNavigator = htmlDocument.CreateNavigator().SelectSingleNode(xPath);
			if (xPathNavigator == null)
				throw new InvalidDataException("The XPath matched no node");

			return xPathNavigator.Value;
		}
	}
}
