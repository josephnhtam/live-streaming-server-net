export interface ErrorResponse {
  message: string;
  errors: { [key: string]: string };
}
