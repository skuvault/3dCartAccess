﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LINQtoCSV;
using Netco.Logging;
using NUnit.Framework;
using ThreeDCartAccess;
using ThreeDCartAccess.RestApi.Models.Configuration;
using ThreeDCartAccess.RestApi.Models.Product.GetProducts;

namespace ThreeDCartAccessTests.Products
{
	public class RestApiProductTests
	{
		private IThreeDCartFactory ThreeDCartFactory;
		private ThreeDCartConfig Config;

		[ SetUp ]
		public void Init()
		{
			NetcoLogger.LoggerFactory = new ConsoleLoggerFactory();
			const string credentialsFilePath = @"..\..\Files\RestApiThreeDCartCredentials.csv";

			var cc = new CsvContext();
			var testConfig = cc.Read< RestApiTestConfig >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true } ).FirstOrDefault();

			if( testConfig != null )
			{
				this.ThreeDCartFactory = new ThreeDCartFactory( testConfig.PrivateKey );
				this.Config = new ThreeDCartConfig( testConfig.StoreUrl, testConfig.Token, testConfig.TimeZone );
			}
		}

		[ Test ]
		public void IsGetProducts()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var result = service.IsGetProducts();

			result.Should().BeTrue();
		}

		[ Test ]
		public void GetProducts()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var result = service.GetProducts();
			var product = result.FirstOrDefault( x => x.SKUInfo.SKU == "SAMPLE-1003" );

			result.Should().NotBeNull();
			result.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetProducts2()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var result = new List< ThreeDCartProduct >();
			service.GetProducts( x => result.Add( x ) );

			result.Should().NotBeNull();
			result.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetProductsAsync()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var result = await service.GetProductsAsync();

			result.Should().NotBeNull();
			result.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetProductsAsync2()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var result = new List< ThreeDCartProduct >();
			await service.GetProductsAsync( x => result.Add( x ) );

			result.Should().NotBeNull();
			result.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetInventory()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var result = service.GetInventory();

			result.Should().NotBeNull();
			result.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetInventoryAsync()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var result = await service.GetInventoryAsync();

			result.Should().NotBeNull();
			result.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void UpdateInventory()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var allProducts = service.GetInventory();
			var product = allProducts.First( x => x.SKUInfo.SKU == "SAMPLE-1003" );
			var productForUpdate = new ThreeDCartAccess.RestApi.Models.Product.UpdateInventory.ThreeDCartProduct( product ) { SKUInfo = { Stock = 3 } };
			productForUpdate.AdvancedOptionList[ 0 ].AdvancedOptionStock = 1;
			productForUpdate.AdvancedOptionList[ 1 ].AdvancedOptionStock = 2;

			service.UpdateInventory( productForUpdate );
		}

		[ Test ]
		public async Task UpdateInventoryAsync()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var allProducts = await service.GetInventoryAsync();
			var product = allProducts.First( x => x.SKUInfo.SKU == "SAMPLE-1003" );
			var productForUpdate = new ThreeDCartAccess.RestApi.Models.Product.UpdateInventory.ThreeDCartProduct( product ) { SKUInfo = { Stock = 5 } };
			productForUpdate.AdvancedOptionList[ 0 ].AdvancedOptionStock = 2;
			productForUpdate.AdvancedOptionList[ 1 ].AdvancedOptionStock = 3;

			await service.UpdateInventoryAsync( productForUpdate );
		}

		[ Test ]
		public void BulkUpdateInventory()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var allProducts = service.GetInventory();
			var product = allProducts.First( x => x.SKUInfo.SKU == "SAMPLE-1003" );
			var productForUpdate = new ThreeDCartAccess.RestApi.Models.Product.UpdateInventory.ThreeDCartProduct( product ) { SKUInfo = { Stock = 4 } };
			productForUpdate.AdvancedOptionList[ 0 ].AdvancedOptionStock = 1;
			productForUpdate.AdvancedOptionList[ 1 ].AdvancedOptionStock = 2;
			productForUpdate.AdvancedOptionList[ 2 ].AdvancedOptionStock = 3;
			//productForUpdate.AdvancedOptionList = new List<ThreeDCartAdvancedOption>
			//{
			//	new ThreeDCartAdvancedOption { AdvancedOptionStock = 1 },
			//	new ThreeDCartAdvancedOption { AdvancedOptionStock = 2 }
			//};

			var product2 = allProducts.First( x => x.SKUInfo.SKU == "SAMPLE-1001" );
			var productForUpdate2 = new ThreeDCartAccess.RestApi.Models.Product.UpdateInventory.ThreeDCartProduct( product2 ) { SKUInfo = { Stock = 7 } };
			productForUpdate2.AdvancedOptionList[ 0 ].AdvancedOptionStock = 1;
			productForUpdate2.AdvancedOptionList[ 1 ].AdvancedOptionStock = 2;

			service.UpdateInventory( new List< ThreeDCartAccess.RestApi.Models.Product.UpdateInventory.ThreeDCartProduct > { productForUpdate, productForUpdate2 } );
		}

		[ Test ]
		public async Task BulkUpdateInventoryAsync()
		{
			var service = this.ThreeDCartFactory.CreateRestProductsService( this.Config );
			var allProducts = await service.GetInventoryAsync();
			var product = allProducts.First( x => x.SKUInfo.SKU == "SAMPLE-1003" );
			var productForUpdate = new ThreeDCartAccess.RestApi.Models.Product.UpdateInventory.ThreeDCartProduct( product ) { SKUInfo = { Stock = 2 } };
			productForUpdate.AdvancedOptionList[ 0 ].AdvancedOptionStock = 2;
			productForUpdate.AdvancedOptionList[ 1 ].AdvancedOptionStock = 3;
			productForUpdate.AdvancedOptionList[ 2 ].AdvancedOptionStock = 4;

			var product2 = allProducts.First( x => x.SKUInfo.SKU == "SAMPLE-1001" );
			var productForUpdate2 = new ThreeDCartAccess.RestApi.Models.Product.UpdateInventory.ThreeDCartProduct( product2 ) { SKUInfo = { Stock = 6 } };
			productForUpdate2.AdvancedOptionList[ 0 ].AdvancedOptionStock = 2;
			productForUpdate2.AdvancedOptionList[ 1 ].AdvancedOptionStock = 3;

			await service.UpdateInventoryAsync( new List< ThreeDCartAccess.RestApi.Models.Product.UpdateInventory.ThreeDCartProduct > { productForUpdate, productForUpdate2 } );
		}
	}
}