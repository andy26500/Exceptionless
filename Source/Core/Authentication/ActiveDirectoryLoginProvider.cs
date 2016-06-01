﻿using System;
using System.DirectoryServices;

namespace Exceptionless.Core.Authentication
{
	public class ActiveDirectoryLoginProvider : IDomainLoginProvider {
		private const string AD_EMAIL = "mail";
		private const string AD_FIRSTNAME = "givenName";
		private const string AD_LASTNAME = "sn";
		private const string AD_DISTINGUISHEDNAME = "distinguishedName";
		private const string AD_USERNAME = "sAMAccountName";

		public bool IsLoginValid(string username, string password)
		{
			using (DirectoryEntry de = new DirectoryEntry(Settings.Current.LdapConnectionString, username, password, AuthenticationTypes.Secure))
			{
				using (DirectorySearcher ds = new DirectorySearcher(de, $"(&({AD_USERNAME}={username}))", new[] { AD_DISTINGUISHEDNAME }))
				{
					try
					{
						SearchResult result = ds.FindOne();
						return result != null;
					}
					// Catch "username and password are invalid"
					catch (DirectoryServicesCOMException ex) when (ex.ErrorCode == -2147023570)
					{
						return false;
					}
				}
			}
		}

		public string GetLoginForEmail(string email) {
			using (DirectoryEntry entry = new DirectoryEntry(Settings.Current.LdapConnectionString)) {
				using (DirectorySearcher searcher = new DirectorySearcher(entry, $"(&({AD_EMAIL}={email}))", new[] { AD_USERNAME })) {
					SearchResult result = searcher.FindOne();
					return result?.Properties[AD_USERNAME][0].ToString();
				}
			}
		}

		public string GetEmailForLogin(string username) {
			SearchResult searchResult = LookupUser(username);

			return searchResult.Properties[AD_EMAIL][0].ToString();
		}

		public string GetNameForLogin(string username) {
			SearchResult searchResult = LookupUser(username);

			return searchResult.Properties[AD_FIRSTNAME][0] + " " + searchResult.Properties[AD_LASTNAME];
		}

		private SearchResult LookupUser(string username) {
			using (DirectoryEntry entry = new DirectoryEntry(Settings.Current.LdapConnectionString)) {
				using (DirectorySearcher searcher = new DirectorySearcher(entry, $"(&({AD_USERNAME}={username}))", new[] { AD_FIRSTNAME, AD_LASTNAME, AD_EMAIL })) {
					return searcher.FindOne();
				}
			}
		}
	}
}
