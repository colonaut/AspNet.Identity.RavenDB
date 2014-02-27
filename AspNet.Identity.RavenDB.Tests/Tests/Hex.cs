﻿using System;
using Xunit;

namespace AspNet.Identity.RavenDB.Tests
{
	public class Hex
	{
		[Fact]
		public void Test_Hex_Roundtrip()
		{
			byte[] randomBytes = new byte[4096];
			Random rand = new Random();
			rand.NextBytes(randomBytes);

			string hex = Util.ToHex(randomBytes);
			Assert.Equal(hex.Length, 4096*2);

			byte[] roundtrip = Util.FromHex(hex);

			Assert.Equal(roundtrip, randomBytes);
		}

		[Fact]
		public void Hex_case_doesnt_matter()
		{
			byte[] b1 = Util.FromHex("0123456789ABCDEFabcdef0123456789");
			byte[] b2 = Util.FromHex("0123456789abcdefABCDEF0123456789");

			Assert.Equal(b1, b2);
		}
	}
}
