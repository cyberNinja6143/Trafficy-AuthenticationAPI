# Trafficy Authentication API

## Overview

The Trafficy Authentication API is a key component of the backend that facilitates user login through the frontend interface.

## Purpose

The goal of this application is to empower users by reducing their reliance on large corporations, such as Google, for traffic data. Trafficy aims to provide users with real-time information on traffic accidents in their area, enabling them to avoid disruptions on their usual routes. The core idea is that while drivers are familiar with their routes, they are often unaware of traffic incidents that may have occurred, affecting their commute. By providing a clear view of accident locations, the app helps users reach their destinations more efficiently.

We will gather traffic data from the TomTom API. However, if the accuracy of the traffic data is ever called into question by a significant number of users, they will have the ability to report inaccuracies. Should enough reports accumulate, the developers will consider switching to a different data provider.

## Commitment to User Privacy

Trafficy is a free service and will remain free indefinitely. We are committed to protecting user privacy and will not collect any unnecessary personal information. Only essential login details will be stored, and sensitive information such as user addresses will be handled on the frontend or directly on the user's device. This information will only be used when the user specifically requests traffic data.

We will not store sensitive personal information, such as user addresses, on our backend systems. All traffic-related data will be used solely for the purpose of providing traffic insights and improving user experience.

## Important Notes Before Contributing

Please review the following points before making any commits:

1. **Accurate Data Reporting**: The primary purpose of this app is to provide accurate traffic data. If users encounter discrepancies in the traffic data, they are encouraged to report these issues. The developers will use these reports to improve data accuracy.
   
2. **User Privacy**: As stated, the app will not collect personal or sensitive information beyond what is necessary for user authentication. Any address or sensitive data will be stored securely on the frontend or user devices.

3. **Free Service**: This app is intended to remain free to all users, without any hidden fees or premium features.

4. **Data Provider**: The app currently utilizes the TomTom API for traffic data. However, we reserve the right to change the provider if the data accuracy or service quality becomes an issue.

## Contributing

We welcome contributions to the Trafficy Authentication API project. Please ensure that you:

- Follow proper code formatting and conventions.
- Test all features thoroughly.
- Report any bugs or issues to the development team.

Thank you for helping us build a better, more efficient traffic data service!
