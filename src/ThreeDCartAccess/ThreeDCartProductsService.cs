﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using ThreeDCartAccess.Misc;
using ThreeDCartAccess.Models.Configuration;
using ThreeDCartAccess.Models.Product;
using ThreeDCartAccess.ThreeDCartAdvancedService;
using ThreeDCartAccess.ThreeDCartService;

namespace ThreeDCartAccess
{
	public class ThreeDCartProductsService: IThreeDCartProductsService
	{
		private readonly ThreeDCartConfig _config;
		private readonly cartAPISoapClient _service;
		private readonly cartAPIAdvancedSoapClient _advancedService;
		private readonly WebRequestServices _webRequestServices;
		private const int BatchSize = 100;
		private const int BatchSizeAdvanced = 500;

		public ThreeDCartProductsService( ThreeDCartConfig config )
		{
			Condition.Requires( config, "config" ).IsNotNull();

			this._config = config;
			this._service = new cartAPISoapClient();
			this._advancedService = new cartAPIAdvancedSoapClient();
			this._webRequestServices = new WebRequestServices();
		}

		public bool IsGetProducts()
		{
			try
			{
				var result = this._service.getProduct( this._config.StoreUrl, this._config.UserKey, BatchSize, 1, "", "" );
				var parsedResult = this._webRequestServices.ParseResult< ThreeDCartProducts >( "IsGetProducts", this._config, result );
				return true;
			}
			catch( Exception )
			{
				return false;
			}
		}

		public IEnumerable< ThreeDCartProduct > GetProducts()
		{
			var result = new List< ThreeDCartProduct >();
			for( var i = 1;; i += BatchSize )
			{
				var portion = this._webRequestServices.Get< ThreeDCartProducts >( this._config,
					() => this._service.getProduct( this._config.StoreUrl, this._config.UserKey, BatchSize, i, "", "" ) );
				if( portion == null )
					break;

				result.AddRange( portion.Products );
				if( portion.Products.Count != BatchSize )
					break;
			}

			return result;
		}

		public async Task< IEnumerable< ThreeDCartProduct > > GetProductsAsync()
		{
			var result = new List< ThreeDCartProduct >();
			for( var i = 1;; i += BatchSize )
			{
				var portion = await this._webRequestServices.GetAsync< ThreeDCartProducts >( this._config,
					async () => ( await this._service.getProductAsync( this._config.StoreUrl, this._config.UserKey, BatchSize, i, "", "" ) ).Body.getProductResult );
				if( portion == null )
					break;

				result.AddRange( portion.Products );
				if( portion.Products.Count != BatchSize )
					break;
			}

			return result;
		}

		public bool IsGetInventory()
		{
			try
			{
				var sql = ScriptsBuilder.GetInventory( BatchSizeAdvanced, 1 );
				var result = this._advancedService.runQuery( this._config.StoreUrl, this._config.UserKey, sql, "" );
				var parsedResult = this._webRequestServices.ParseResult< ThreeDCartInventories >( "IsGetInventory", this._config, result );
				return true;
			}
			catch( Exception )
			{
				return false;
			}
		}

		public IEnumerable< ThreeDCartInventory > GetInventory()
		{
			var result = new List< ThreeDCartInventory >();
			for( var i = 1;; i += BatchSizeAdvanced )
			{
				var sql = ScriptsBuilder.GetInventory( BatchSizeAdvanced, i );
				var portion = this._webRequestServices.Get< ThreeDCartInventories >( this._config,
					() => this._advancedService.runQuery( this._config.StoreUrl, this._config.UserKey, sql, "" ) );
				if( portion == null )
					break;

				result.AddRange( portion.Inventory );
				if( portion.Inventory.Count != BatchSizeAdvanced )
					break;
			}

			return result;
		}

		public async Task< IEnumerable< ThreeDCartInventory > > GetInventoryAsync()
		{
			var result = new List< ThreeDCartInventory >();
			for( var i = 1;; i += BatchSizeAdvanced )
			{
				var sql = ScriptsBuilder.GetInventory( BatchSizeAdvanced, i );
				var portion = await this._webRequestServices.GetAsync< ThreeDCartInventories >( this._config,
					async () => ( await this._advancedService.runQueryAsync( this._config.StoreUrl, this._config.UserKey, sql, "" ) ).Body.runQueryResult );
				if( portion == null )
					break;

				result.AddRange( portion.Inventory );
				if( portion.Inventory.Count != BatchSizeAdvanced )
					break;
			}

			return result;
		}

		public IEnumerable< ThreeDCartUpdateInventory > UpdateInventory( IEnumerable< ThreeDCartUpdateInventory > inventory, bool updateProductTotalStock = false )
		{
			var result = new List< ThreeDCartUpdateInventory >();
			var productsWithOptions = new List< string >();
			foreach( var inv in inventory )
			{
				ThreeDCartUpdateInventory response;
				if( string.IsNullOrEmpty( inv.OptionCode ) )
					response = this.UpdateProductInventory( inv );
				else
				{
					response = this.UpdateProductOptionInventory( inv );
					if( !productsWithOptions.Contains( inv.ProductId ) )
						productsWithOptions.Add( inv.ProductId );
				}
				if( response != null )
					result.Add( response );
			}

			if( !updateProductTotalStock || productsWithOptions.Count == 0 )
				return result;

			var updatedInventory = this.GetInventory().ToList();
			foreach( var product in productsWithOptions )
			{
				var sum = updatedInventory.Where( x => x.IsProductOption && x.ProductId == product && x.OptionStock > 0 ).Sum( x => x.OptionStock );
				var response = this.UpdateProductInventory( new ThreeDCartUpdateInventory { ProductId = product, NewQuantity = sum } );
				if( response != null )
					result.Add( response );
			}

			return result;
		}

		public async Task< IEnumerable< ThreeDCartUpdateInventory > > UpdateInventoryAsync( IEnumerable< ThreeDCartUpdateInventory > inventory, bool updateProductTotalStock = false )
		{
			var result = new List< ThreeDCartUpdateInventory >();
			var productsWithOptions = new List< string >();
			foreach( var inv in inventory )
			{
				ThreeDCartUpdateInventory response;
				if( string.IsNullOrEmpty( inv.OptionCode ) )
					response = await this.UpdateProductInventoryAsync( inv );
				else
				{
					response = await this.UpdateProductOptionInventoryAsync( inv );
					if( !productsWithOptions.Contains( inv.ProductId ) )
						productsWithOptions.Add( inv.ProductId );
				}
				if( response != null )
					result.Add( response );
			}

			if( !updateProductTotalStock || productsWithOptions.Count == 0 )
				return result;

			var updatedInventory = this.GetInventory().ToList();
			foreach( var product in productsWithOptions )
			{
				var sum = updatedInventory.Where( x => x.IsProductOption && x.ProductId == product && x.OptionStock > 0 ).Sum( x => x.OptionStock );
				var response = await this.UpdateProductInventoryAsync( new ThreeDCartUpdateInventory { ProductId = product, NewQuantity = sum } );
				if( response != null )
					result.Add( response );
			}

			return result;
		}

		private ThreeDCartUpdateInventory UpdateProductInventory( ThreeDCartUpdateInventory inventory )
		{
			var result = this._webRequestServices.Submit< ThreeDCartUpdateInventory >( this._config,
				() => this._service.updateProductInventory( this._config.StoreUrl, this._config.UserKey, inventory.ProductId, inventory.NewQuantity, true, "" ) );
			return result;
		}

		private async Task< ThreeDCartUpdateInventory > UpdateProductInventoryAsync( ThreeDCartUpdateInventory inventory )
		{
			var result = await this._webRequestServices.SubmitAsync< ThreeDCartUpdateInventory >( this._config,
				async () => ( await this._service.updateProductInventoryAsync( this._config.StoreUrl, this._config.UserKey, inventory.ProductId, inventory.NewQuantity, true, "" ) )
					.Body.updateProductInventoryResult );
			return result;
		}

		private string GetSqlForUpdateProductOptionInventory( ThreeDCartUpdateInventory inventory )
		{
			return string.Format( "UPDATE options_Advanced SET AO_Stock = {0} WHERE AO_Sufix = '{1}'", inventory.NewQuantity, inventory.OptionCode );
		}

		private ThreeDCartUpdateInventory UpdateProductOptionInventory( ThreeDCartUpdateInventory inventory )
		{
			var sql = this.GetSqlForUpdateProductOptionInventory( inventory );
			var result = this._webRequestServices.Submit< ThreeDCartUpdatedOptionInventory >( this._config,
				() => this._advancedService.runQuery( this._config.StoreUrl, this._config.UserKey, sql, "" ) );
			return inventory;
		}

		private async Task< ThreeDCartUpdateInventory > UpdateProductOptionInventoryAsync( ThreeDCartUpdateInventory inventory )
		{
			var sql = this.GetSqlForUpdateProductOptionInventory( inventory );
			var result = await this._webRequestServices.SubmitAsync< ThreeDCartUpdatedOptionInventory >( this._config,
				async () => ( await this._advancedService.runQueryAsync( this._config.StoreUrl, this._config.UserKey, sql, "" ) ).Body.runQueryResult );
			return inventory;
		}
	}
}