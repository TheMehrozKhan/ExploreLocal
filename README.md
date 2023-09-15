<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>ExploreLocal Project Setup Guide</title>
</head>
<body>
    <h1>ExploreLocal Project Setup Guide</h1>

    <h2>Prerequisites</h2>
    <ul>
        <li>Database: Download the project database from <a href="https://cutt.ly/OwjPXR0Y">this link</a>.</li>
        <li>Project Files: Download the ExploreLocal project files from GitHub using <a href="https://cutt.ly/mwkhMEb2">this link</a>.</li>
        <li>Visual Studio: Ensure you have Visual Studio 2019 or a later version installed on your system.</li>
    </ul>

    <h2>Database Setup</h2>
    <ol>
        <li>Download the database file from the provided link.</li>
        <li>Open SQL Server Management Studio (SSMS).</li>
        <li>Connect to your SQL Server instance.</li>
        <li>In SSMS, right-click on "Databases" in the Object Explorer, and select "Restore Database."</li>
        <li>Choose "Device" and browse for the downloaded database file.</li>
        <li>Click "OK" to restore the database.</li>
    </ol>

    <h2>Project Setup</h2>
    <ol>
        <li>Open Visual Studio.</li>
        <li>Open the ExploreLocal project that you downloaded from GitHub.</li>
        <li>Make sure to configure your connection string in the Web.config file to match your database setup.</li>
        <li>Open the "Models" folder and create your model. Ensure you've configured your entities and database connections according to your needs.</li>
        <li>In the model class Tbl_Destination, add the following line at the top:</li>
    </ol>
    <pre><code>using System.Web.Mvc;</code></pre>
    <p>Add the [AllowHtml] attribute just above the GoogleStreetViewURL, Destination_Highlights, or Description property, like this:</p>
    <pre><code>[AllowHtml]
public string GoogleStreetViewURL { get; set; }</code></pre>
    <p>Also, add the following line within the Tbl_Destination class to establish a relationship with the Tbl_Expert entity:</p>
    <pre><code>public Tbl_Expert Expert { get; set; }</code></pre>
    <p>Open the Tbl_Bookings class and add the following line within the class to include a TourState property:</p>
    <pre><code>public string TourState { get; set; }</code></pre>
    <ol start="6">
        <li>Click on the "Build" menu at the top of Visual Studio.</li>
        <li>Select "Clean ExploreLocal."</li>
        <li>Click on "Build ExploreLocal."</li>
        <li>After building successfully, click on "Clean Solution."</li>
        <li>Finally, click on "Build ExploreLocal Solution."</li>
        <li>Run the project by clicking the "Start" button (green arrow) in Visual Studio.</li>
    </ol>

    <h2>Usage</h2>
    <p>You can now access the ExploreLocal project by opening a web browser and navigating to the appropriate URL (usually http://localhost:port/).</p>

    <h2>Admin Access</h2>
    <p>To access the admin panel, use the following credentials:</p>
    <p>Username: [Insert Username Here]</p>
    <p>Password: [Insert Password Here]</p>

    <p>That's it! You've successfully installed and set up the ExploreLocal project. If you have any questions or encounter any issues, please don't hesitate to reach out for assistance.</p>
</body>
</html>
