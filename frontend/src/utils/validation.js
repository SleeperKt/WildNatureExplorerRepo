/**
 * Validation rules matching backend validators
 * (LoginUserDtoValidator, RegisterUserDtoValidator)
 */

const VALIDATION_RULES = {
  email: {
    required: 'Email is required.',
    format: 'Email format is invalid. Use format: name@gmail.com',
    maxLength: 'Email cannot exceed 32 characters.',
  },
  password: {
    required: 'Password is required.',
    minLength: 'Password must be at least 8 characters.',
    maxLength: 'Password cannot exceed 20 characters.',
    uppercase: 'Password must contain at least one uppercase letter.',
    lowercase: 'Password must contain at least one lowercase letter.',
    digits: 'Password must contain at least 2 digits.',
    special:
      'Password must contain at least one special character (! ? @ # $).',
  },
  firstName: {
    required: 'FirstName is required.',
    maxLength: 'FirstName cannot exceed 16 characters.',
  },
  lastName: {
    required: 'LastName is required.',
    maxLength: 'LastName cannot exceed 16 characters.',
  },
};

/**
 * Validate email
 */
export const validateEmail = (email) => {
  if (!email || email.trim() === '') {
    return VALIDATION_RULES.email.required;
  }
  if (!isValidEmailFormat(email)) {
    return VALIDATION_RULES.email.format;
  }
  if (email.length > 32) {
    return VALIDATION_RULES.email.maxLength;
  }
  return null;
};

/**
 * Validate password
 */
export const validatePassword = (password) => {
  if (!password || password.trim() === '') {
    return VALIDATION_RULES.password.required;
  }
  if (password.length < 8) {
    return VALIDATION_RULES.password.minLength;
  }
  if (password.length > 20) {
    return VALIDATION_RULES.password.maxLength;
  }
  if (!/[A-Z]/.test(password)) {
    return VALIDATION_RULES.password.uppercase;
  }
  if (!/[a-z]/.test(password)) {
    return VALIDATION_RULES.password.lowercase;
  }
  if (!/\d.*\d/.test(password)) {
    return VALIDATION_RULES.password.digits;
  }
  if (!/[!@#$?]/.test(password)) {
    return VALIDATION_RULES.password.special;
  }
  return null;
};

/**
 * Validate first name
 */
export const validateFirstName = (firstName) => {
  if (!firstName || firstName.trim() === '') {
    return VALIDATION_RULES.firstName.required;
  }
  if (firstName.length > 16) {
    return VALIDATION_RULES.firstName.maxLength;
  }
  return null;
};

/**
 * Validate last name
 */
export const validateLastName = (lastName) => {
  if (!lastName || lastName.trim() === '') {
    return VALIDATION_RULES.lastName.required;
  }
  if (lastName.length > 16) {
    return VALIDATION_RULES.lastName.maxLength;
  }
  return null;
};

/**
 * Validate login form
 */
export const validateLoginForm = (email, password) => {
  const errors = {};

  const emailError = validateEmail(email);
  if (emailError) errors.email = emailError;

  const passwordError = validatePassword(password);
  if (passwordError) errors.password = passwordError;

  return errors;
};

/**
 * Validate register form
 */
export const validateRegisterForm = (email, password, firstName, lastName) => {
  const errors = {};

  const emailError = validateEmail(email);
  if (emailError) errors.email = emailError;

  const passwordError = validatePassword(password);
  if (passwordError) errors.password = passwordError;

  const firstNameError = validateFirstName(firstName);
  if (firstNameError) errors.firstName = firstNameError;

  const lastNameError = validateLastName(lastName);
  if (lastNameError) errors.lastName = lastNameError;

  return errors;
};

/**
 * Helper: Check if email format is valid
 */
const isValidEmailFormat = (email) => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};
