﻿using System.Collections.Generic;
using System.Security.Cryptography;
using NeoSharp.Core.Caching;
using NeoSharp.Core.Models;
using NeoSharp.Core.Types;

namespace NeoSharp.Core.Blockchain
{
    public interface IBlockchain
    {
        UInt256 CurrentBlockHash { get; }
        UInt256 CurrentHeaderHash { get; }
        uint HeaderHeight { get; }
        uint Height { get; }

        /// <summary>
        /// Add the specified block to the blockchain
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        bool AddBlock(Block block);

        /// <summary>
        /// Determine whether the specified block is contained in the blockchain
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        bool ContainsBlock(UInt256 hash);

        /// <summary>
        /// Determine whether the specified transaction is included in the blockchain
        /// </summary>
        /// <param name="hash">交易编号</param>
        /// <returns>如果包含指定交易则返回true</returns>
        bool ContainsTransaction(UInt256 hash);

        bool ContainsUnspent(CoinReference input);
        bool ContainsUnspent(UInt256 hash, ushort index);
        MetaDataCache<T> GetMetaData<T>() where T : class, ISerializable, new();

        /// <summary>
        /// Return the corresponding block information according to the specified height
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        Block GetBlock(uint height);

        /// <summary>
        /// Return the corresponding block information according to the specified height
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        Block GetBlock(UInt256 hash);

        /// <summary>
        /// Returns the hash of the corresponding block based on the specified height
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        UInt256 GetBlockHash(uint height);

        /// <summary>
        /// Return the corresponding block header information according to the specified height
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        Header GetHeader(uint height);

        /// <summary>
        /// Returns the corresponding block header information according to the specified hash value
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        Header GetHeader(UInt256 hash);

        ECPoint[] GetValidators();
        IEnumerable<ECPoint> GetValidators(IEnumerable<Transaction> others);

        /// <summary>
        /// Returns the information for the next block based on the specified hash value
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        Block GetNextBlock(UInt256 hash);

        /// <summary>
        /// Returns the hash value of the next block based on the specified hash value
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        UInt256 GetNextBlockHash(UInt256 hash);

        /// <summary>
        /// Returns the total amount of system costs contained in the corresponding block and all previous blocks based on the specified block height
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        long GetSysFeeAmount(uint height);

        /// <summary>
        /// Returns the total amount of system charges contained in the corresponding block and all previous blocks based on the specified block hash value
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        long GetSysFeeAmount(UInt256 hash);

        /// <summary>
        /// Returns the corresponding transaction information according to the specified hash value
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        Transaction GetTransaction(UInt256 hash);

        /// <summary>
        /// Return the corresponding transaction information and the height of the block where the transaction is located according to the specified hash value
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Transaction GetTransaction(UInt256 hash, out int height);

        /// <summary>
        /// Get the corresponding unspent assets based on the specified hash value and index
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        TransactionOutput GetUnspent(UInt256 hash, ushort index);

        IEnumerable<TransactionOutput> GetUnspent(UInt256 hash);

        /// <summary>
        /// Determine if the transaction is double
        /// </summary>
        /// <param name="tx"></param>
        /// <returns></returns>
        bool IsDoubleSpend(Transaction tx);
    }
}