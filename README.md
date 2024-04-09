# **Short Description**
Online Shop Web Application using APS.NET Core Framework.

# **Features**

- four types of users: unregistered user, registered user, collaborator, administrator
- the collaborator user can add products to the store. A request will be sent to the administrator for approval, and the administrator can either approve or reject them. After approval, the products will be visible in the store. 
- products are part of categories
- categories are dynamically created by the administrator, who can add new categories directly from the application interface
- the admin can view, edit, and delete categories as needed 
- a product has a title, description, image, price, rating (1-5 stars), and user reviews
- the collaborators can edit and delete the products they have added
- unregistered users will be redirected to create an account when trying to add a product to the cart. When not logged in, they can only view products and associated comments
- when a user becomes a registered user, they can place orders (add products to the cart) and leave reviews, which they can later edit or delete
- products can be searched by name using a search engine. Products are also found even if a user searches for specific parts of the name
- search engine results can be sorted in ascending or descending order by price and number of stars
- the administrator can delete and edit both products and comments. The administrator can also activate or revoke user rights.

# **Technologies**

- .NET Core MVC framework
- Entity Framework
- HTML/CSS/JavaScript 
- C# language

