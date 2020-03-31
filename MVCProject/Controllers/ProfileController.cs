﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCProject.DataRepositories;
using MVCProject.Models;
using MVCProject.ViewModels;

namespace MVCProject.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IDataRepository<FriendRequest, string> friendRequestRepository;
        private readonly IDataRepository<User, string> userRepository;
        private readonly IDataRepository<Post, int> postRepository;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private User currentUser;

        public ProfileController(IDataRepository<FriendRequest, string> friendRequestRepository,
                                 IDataRepository<User, string> userRepository,
                                 IDataRepository<Post, int> postRepository,
                                 SignInManager<User> signInManager,
                                 UserManager<User> userManager)
        {
            this.friendRequestRepository = friendRequestRepository;
            this.userRepository = userRepository;
            this.postRepository = postRepository;
            this.signInManager = signInManager;
            this.userManager = userManager;

        }

        private User GetCurrentUser()
        {
            if (currentUser == null)
            {
                currentUser = userManager.FindByIdAsync(userManager.GetUserId(User)).Result;
                currentUser.Posts = postRepository.SelectByUserId(currentUser.Id);
                currentUser.FriendRequestReceivers = friendRequestRepository.GetFriendRequestReceivers(currentUser.Id);
                currentUser.FriendRequestSenders = friendRequestRepository.GetFriendRequestSenders(currentUser.Id);
            }
            return currentUser;
        }

        [AllowAnonymous]
        public IActionResult Search(string searchText)
        {
            ViewBag.CurrentUser = GetCurrentUser();
            if (searchText == null || searchText == "")
            {
                var searchList = userRepository.SelectAll();
                searchList.Remove(GetCurrentUser());
                return View(searchList);
            }
            string[] fullname = searchText.Split(" ");
            List<User> users = null;
            if (fullname.Length == 1)
            {
                users = userRepository.SelectAll().Where(u => u.UserFirstName.Contains(searchText) || u.UserLastName.Contains(searchText)).ToList();
            }
            else if (fullname.Length > 1)
            {
                users = userRepository.SelectAll().Where(u => u.UserFirstName.Contains(fullname[0]) || u.UserLastName.Contains(fullname[1]) || u.UserFirstName.Contains(fullname[1]) || u.UserLastName.Contains(fullname[0])).ToList();
            }

            if (users == null)
            {
                return NotFound();
            }
            users.Remove(GetCurrentUser());
            
            return View(users);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            return View(GetCurrentUser());
        }

        [HttpPost]
        public IActionResult Profile(string newPost, string ImageFile)
        {
            if (newPost != null || ImageFile != null)
            {
                Post p = new Post()
                {
                    PostContent = newPost,
                    PostDateTime = DateTime.Now,
                    UserId = GetCurrentUser().Id,
                    PostImage = ImageFile,
                    IsDeleted = false
                };
                postRepository.Insert(p);
            }
            return RedirectToAction("Profile");
        }

        [HttpPost, ActionName("ProfilePic")]
        public IActionResult ProfilePic(string ImageFile)
        {
            if (ImageFile != null)
            {
                GetCurrentUser().UserPicture = ImageFile;
                userRepository.Update(GetCurrentUser().Id, GetCurrentUser());
            }
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult Edit(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                GetCurrentUser().ProfileViewModel = model;
                userRepository.Update(GetCurrentUser().Id, GetCurrentUser());
            }

            return View("Profile",GetCurrentUser());
        }

        public IActionResult Friend_Profile(string id)
        {
            ViewBag.CurrentUser = GetCurrentUser();
            if (!userRepository.IsExist(id))
            {
                return NotFound();
            }

            var user = userRepository.SelectById(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        public void AddFriend(string id)
        {
            if (id != null)
            {
                var friendRequest = new FriendRequest() { SenderId = GetCurrentUser().Id, ReceiverId = id, State = FriendRequestState.Pending };
                //_context.FriendRequests.Add(friendRequest);
                friendRequestRepository.Insert(friendRequest);
                //_context.SaveChanges();
            }
        }

        public void RemoveFriend(string id)
        {
        
            if (id != null)
            {
                var friendRequest = friendRequestRepository.SelectAll().SingleOrDefault(m => (m.SenderId == GetCurrentUser().Id && m.ReceiverId == id) || (m.SenderId == id && m.ReceiverId == GetCurrentUser().Id));
                if (friendRequest != null)
                {
                    friendRequestRepository.Delete(friendRequest);
                    
                }
            }
        }
        public void AcceptFriend(string id)
        {
            if (id != null)
            {
                var friendRequest = friendRequestRepository.SelectAll().SingleOrDefault(m => m.SenderId == id && m.ReceiverId == GetCurrentUser().Id);

                if (friendRequest != null)
                {
                    friendRequest.State = FriendRequestState.Accepted;
                    friendRequestRepository.Update("", friendRequest);
                }
            }
        }

    }
}