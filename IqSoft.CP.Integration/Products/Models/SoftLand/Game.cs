using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.SoftLand
{
	public class Game
	{
		[JsonProperty(PropertyName = "Id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "Description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "GameCode")]
		public string GameCode { get; set; }

		[JsonProperty(PropertyName = "Rtp")]
		public Decimal Rtp { get; set; }

		[JsonProperty(PropertyName = "MinBet")]
		public Decimal MinBet { get; set; }

		[JsonProperty(PropertyName = "MaxBet")]
		public Decimal MaxBet { get; set; }

		[JsonProperty(PropertyName = "MaxExposure")]
		public Decimal MaxExposure { get; set; }

		[JsonProperty(PropertyName = "HasFreeSpin")]
		public bool HasFreeSpin { get; set; }

		[JsonProperty(PropertyName = "HasJackpot")]
		public bool HasJackpot { get; set; }

		[JsonProperty(PropertyName = "HasDemoMode")]
		public bool HasDemoMode { get; set; }

		[JsonProperty(PropertyName = "HasLobby")]
		public bool HasLobby { get; set; }

		[JsonProperty(PropertyName = "IsBranded")]
		public bool IsBranded { get; set; }

		[JsonProperty(PropertyName = "IsAnimated")]
		public bool IsAnimated { get; set; }

		[JsonProperty(PropertyName = "State")]
		public string State { get; set; }

		[JsonProperty(PropertyName = "LogoPath")]
		public string LogoPath { get; set; }

		[JsonProperty(PropertyName = "BackgroundPath")]
		public string BackgroundPath { get; set; }

		[JsonProperty(PropertyName = "AnimatedPath")]
		public string AnimatedPath { get; set; }

		[JsonProperty(PropertyName = "CategoryNames")]
		public List<string> CategoryNames { get; set; }

		[JsonProperty(PropertyName = "Provider")]
		public Provider Provider { get; set; }
	}

	public class Provider
	{
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "UnderMaintenance")]
		public bool UnderMaintenance { get; set; }

		[JsonProperty(PropertyName = "Description")]
		public string Description { get; set; }
	}
}
