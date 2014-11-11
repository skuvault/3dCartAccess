﻿using LINQtoCSV;

namespace ThreeDCartAccessTests
{
	internal class TestConfig
	{
		[ CsvColumn( Name = "StoreUrl", FieldIndex = 1 ) ]
		public string StoreUrl{ get; set; }

		[ CsvColumn( Name = "UserKey", FieldIndex = 2 ) ]
		public string UserKey{ get; set; }
	}
}