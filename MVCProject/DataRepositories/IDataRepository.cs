﻿using MVCProject.Models;
using System.Collections.Generic;

namespace MVCProject.DataRepositories
{
    public interface IDataRepository<T,K>
    {
        List<T> SelectAll();
        T SelectById(K id);
        void Insert(T t);
        void Update(K id, T t);
        void DeleteById(K id);
        bool IsExist(K id);
        List<FriendRequest> GetFriendRequestReceivers(string userId) => null; 
        List<FriendRequest> GetFriendRequestSenders(string userId) => null;
        List<Post> SelectByUserId(string id) => null;
        void Delete(T t);

    }
}
