# 🛡️ SecureVault

A local-first, encrypted file vault application for Windows, built with C# and WPF. This project aims to provide a secure environment, similar to a macOS Finder, for managing sensitive files, completely isolated from the host operating system.

---

## 📖 About The Project

SecureVault is a desktop application designed to be a secure, local-only file safe. Users can log in with a master password or PIN to access a vault that functions like a familiar file explorer.

The core principle is security through isolation and encryption:
* **Local-First:** All data is stored on your local machine, not on any cloud server.
* **Encrypted:** All files added to the vault are encrypted.
* **Isolated:** The vault's contents are not accessible through the standard Windows File Explorer or any other application.

## 🎯 Project Goals & Features

This application is being built to include the following features:

* **Secure Authentication:** A robust login screen requiring a password or PIN to unlock the vault.
* **Encrypted Storage:** All files and folder structures within the vault will be encrypted (using `CryptoService.cs`).
* **Familiar UI:** A "macOS Finder" style interface for intuitive file management.
* **Full File Operations:** Implement all general file explorer features:
    * Create, Delete, and Rename files and folders.
    * Add files from the local system into the vault.
    * Right-click context menus with options like `Copy`, `Paste`, `Cut`, `Delete`, and `Properties`.
* **Total Isolation:** Files inside the vault cannot be accessed from outside the SecureVault application.

## 🛠️ Tech Stack & Architecture

This project is built using:

* **Language:** **C#**
* **Framework:** **.NET Framework 4.8**
* **Platform:** **.NET Desktop Development (WPF)**
* **IDE:** **Visual Studio**

### Architecture (MVVM)

The project follows the **Model-View-ViewModel (MVVM)** design pattern to ensure a clean separation of concerns.

* **`/Views/`**: Contains all the UI (XAML) files, such as `PasswordView.xaml` and `VaultView.xaml`. These are responsible for the "look" of the application.
* **`/ViewModels/`**: Contains the logic and state for the Views (e.g., `VaultViewModel.cs`, `PasswordViewModel.cs`). They handle user interactions and data preparation.
* **`/Models/`**: Contains the data structures, such as `FileItem.cs`, which represents a file or folder in the vault.
* **`/Services/`**: Holds the core business logic, decoupled from the UI.
    * `CryptoService.cs`: For handling all encryption and decryption.
    * `VaultService.cs`: For managing the vault's file system operations.
    * `FileIconService.cs`: For retrieving file-appropriate icons.
* **`/Helpers/` & `/Converters/`**: Utility classes like `RelayCommand.cs` and value converters to support the MVVM architecture.

## 🚀 Getting Started (Building from Source)

To get a local copy up and running for development:

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/PiyushJaiswall/SecureVault.git
    ```
2.  **Open in Visual Studio:**
    * Navigate to the cloned directory and open `SecureVault.sln`.
    * Ensure you have the **".NET desktop development"** workload installed in Visual Studio.
    * Ensure your environment is set up for **.NET Framework 4.8**.
3.  **Build the Project:**
    Press `Ctrl+Shift+B` or go to `Build > Build Solution` to restore dependencies and compile the application.
4.  **Run:**
    Set `SecureVault` as the startup project and press `F5` to debug.

## 🗺️ Roadmap & Next Steps

Based on the current issues, the immediate priorities are:

* [ ] **Restore Core Features:** Re-implement the "gone" features.
* [ ] **Fix UI Connectivity:** Properly connect the `MainWindow.xaml` (with its `Login` and `Vault` tabs) to their respective ViewModels.
* [ ] **Implement Services:** Wire up the `CryptoService` and `VaultService` to the ViewModels.
* [ ] **Code Refactoring:** Clean up the codebase to ensure all components are written correctly and interact as intended.

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".

1.  Fork the Project
2.  Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3.  Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4.  Push to the Branch (`git push origin feature/AmazingFeature`)
5.  Open a Pull Request

## 📧 Contact

Piyush Jaiswall - [@PiyushJaiswall](https://github.com/PiyushJaiswall)

Project Link: [https://github.com/PiyushJaiswall/SecureVault](https://github.com/PiyushJaiswall/SecureVault)
